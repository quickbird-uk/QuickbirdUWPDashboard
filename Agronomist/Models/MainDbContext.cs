namespace Agronomist.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using JetBrains.Annotations;
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Metadata;
    using NetLib;
    using Newtonsoft.Json;

    public class MainDbContext : DbContext
    {
        private List<object> _dbos;
        public DbSet<ControlType> ControlTypes { get; set; }
        public DbSet<ParamAtPlace> ParamAtPlaces { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<PlacementType> PlacementTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<ControlHistory> ControlHistories { get; set; }
        public DbSet<Controllable> Controllables { get; set; }
        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }
        public DbSet<Greenhouse> Greenhouses { get; set; }
        public DbSet<SensorData> SensorDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=maindb.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CropType>()
                .HasKey(c => c.Name);

            modelBuilder.Entity<ControlHistory>()
                .HasKey(ct => new {ct.ControllableID, ct.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasOne(gh => gh.Person)
                .WithMany(p => p.Greenhouses)
                .HasForeignKey(gh => gh.PersonId)
                .IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<Controllable>()
            //    .HasOne(ct => ct.Relay)
            //    .WithOne(r => r.)
            //    .IsRequired(false)
            //    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SensorData>()
                .HasKey(sd => new {sd.SensorID, sd.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasMany(gh => gh.SensorData)
                .WithOne(sd => sd.Greenhouse)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }

        /// <summary>
        ///     Returns null on sucessful merge.
        /// </summary>
        /// <typeparam name="TPoco"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="cred"></param>
        /// <returns></returns>
        public async Task<string> FetchTableAndDeserialise<TPoco>(string tableName, Creds cred = null)
            where TPoco : class
        {
            // Step 1: Request
            const string baseUrl = "https://ghapi46azure.azurewebsites.net/api";
            string response;
            if (cred == null)
                response = await Request.RequestTable(baseUrl, tableName);
            else
                response = await Request.RequestTable(baseUrl, tableName, cred);

            if (null == response)
            {
                Debug.WriteLine($"Request failed: {tableName}, creds {null == cred}.");
            }

            // Step 2: Deserialise
            List<TPoco> updatesFromServer;
            try
            {
                updatesFromServer = JsonConvert.DeserializeObject<List<TPoco>>(response);
            }
            catch (JsonSerializationException e)
            {
                Debug.WriteLine($"Desserialise falied on response for {tableName}, creds {null == cred}.");
                Debug.WriteLine(e);
                return "Unable to deserialise.";
            }

            // Step 3: Merge
            // Get the DbSet that this request should be inserted into.
            var dbSet = (DbSet<TPoco>) _dbos.First(d => d is DbSet<TPoco>);
            if (typeof(TPoco).GetInterfaces().Contains(typeof(IHasId)))
            {
                var dbSetAsId = dbSet.Select(d => (IHasId) d).AsNoTracking();
                foreach (var entry in updatesFromServer)
                {
                    var existing = dbSetAsId.FirstOrDefault(d => d.ID == ((IHasId) entry).ID);
                    AddOrModify(existing, dbSet, entry);
                }
            }

            SaveChanges();

            return null;
        }

        private static void AddOrModify<TPoco>(IHasId existing, [NotNull]DbSet<TPoco> dbSet, [NotNull]TPoco entry) where TPoco : class
        {
            if (existing == null)
            {
                dbSet.Add(entry, GraphBehavior.SingleObject);
            }
            else
            {
                var x = dbSet.Update(entry, GraphBehavior.SingleObject);
                x.State = EntityState.Modified;
            }
        }

        /// <summary>
        ///     Responds with a status message.
        /// </summary>
        /// <returns></returns>
        public async Task<string> PullAndPopulate()
        {
            //Reds first since they are an independent set, and they do not require authentication.
            // 1.PlacementType
            // 2.Parameter
            // 3.Subsystem
            // 4.Placement_has_Parameter
            // 5.ControlTypes
            //Now for the big part
            // 1.Greenhouse
            // 2.CropType
            // 3.Devices
            // 4.Cycle
            // 5.Sensors
            // 6.Relay
            // 7.Controllable
            // 8.ControlHistory
            // 9.SensorData

            // Names on the API
            //Controllables
            //ControlTypes
            //CropTypes
            //Cycles
            //Greenhouses
            //Parameters
            //ParamsAtPlaces
            //People
            //PlacementTypes
            //Subsystems
            //Values
            _dbos = new List<object>
            {
                ControlTypes,
                ParamAtPlaces,
                Parameters,
                PlacementTypes,
                Subsystems,
                Devices,
                Relays,
                Sensors,
                ControlHistories,
                Controllables,
                CropCycles,
                CropTypes,
                Greenhouses,
                SensorDatas
            };

            var placementType = await FetchTableAndDeserialise<PlacementType>(nameof(PlacementTypes));
            var parameters = await FetchTableAndDeserialise<Parameter>(nameof(Parameters));
            var subsystems = await FetchTableAndDeserialise<Subsystem>(nameof(Subsystems));
            var paramsAtPlaces = await FetchTableAndDeserialise<ParamAtPlace>("ParamsAtPlaces");
            var controlTypes = await FetchTableAndDeserialise<ControlType>(nameof(ControlTypes));

            var responses = new[]
            {
                placementType, parameters, subsystems, paramsAtPlaces, controlTypes
            };

            var fails = responses.Where(r => r != null).ToList();
            return fails.Any() ? string.Join(", ", fails) : null;
        }
    }
}