namespace Agronomist.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Metadata;
    using NetLib;
    using Newtonsoft.Json;

    public class MainDbContext : DbContext
    {
        private List<object> _dbos;
        public DbSet<ControlType> ControlTypes { get; set; }
        public DbSet<ParamAtPlace> ParamsAtPlaces { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<PlacementType> PlacementTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<ControlHistory> ControlHistories { get; set; }
        public DbSet<Controllable> Controllables { get; set; }
        public DbSet<CropCycle> Cycles { get; set; }
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
        /// <typeparam name="TPoco">The POCO type of the table.</typeparam>
        /// <param name="tableName">Name of the table to request.</param>
        /// <param name="cred">Credentials to be used to authenticate with the server. Only required for some types.</param>
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

                return $"Request failed: {tableName}.";
            }


            // Step 2: Deserialise
            List<TPoco> updatesFromServer;
            try
            {
                updatesFromServer = await Task.Run(() => JsonConvert.DeserializeObject<List<TPoco>>(response));
            }
            catch (JsonSerializationException e)
            {
                Debug.WriteLine($"Desserialise falied on response for {tableName}, creds {null == cred}.");
                Debug.WriteLine(e);
                return "Unable to deserialise.";
            }

            // Step 3: Merge
            // Get the DbSet that this request should be inserted into.
            await AddOrModify(updatesFromServer);

            SaveChanges();

            return null;
        }

        /// <summary>
        /// Figures out the real type of the table entitiy, performs checks for existing items and merges data where required.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the entity.</typeparam>
        /// <param name="updatesFromServer">The data recieved from the server.</param>
        /// <returns>Awaitable, the local database queries are done async.</returns>
        private async Task AddOrModify<TPoco>(List<TPoco> updatesFromServer)
            where TPoco : class
        {
            var dbSet = (DbSet<TPoco>) _dbos.First(d => d is DbSet<TPoco>);
            var pocoType = typeof(TPoco);
            foreach (var entry in updatesFromServer)
            {
                TPoco existing = null;
                if (pocoType.GetInterfaces().Contains(typeof(IHasId)))
                {
                    existing =
                        await
                            dbSet.Select(a => a)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => ((IHasId) d).ID == ((IHasId) entry).ID);
                }
                else if (pocoType.GetInterfaces().Contains(typeof(IHasGuid)))
                {
                    existing =
                        await
                            dbSet.Select(a => a)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => ((IHasGuid) d).ID == ((IHasGuid) entry).ID);
                }
                else if (pocoType == typeof(CropType))
                {
                    var x = entry as CropType;
                    existing = dbSet.OfType<CropType>().AsNoTracking().FirstOrDefault(d => d.Name == x.Name) as TPoco;
                }

                if (existing == null)
                {
                    dbSet.Add(entry, GraphBehavior.SingleObject);
                }
                else
                {
                    if (entry is BaseEntity && existing is BaseEntity)
                    {
                        // These types allow local changes. Check date and don't overwrite unless the server has changed.
                        var remoteVersion = entry as BaseEntity;
                        var localVersion = existing as BaseEntity;

                        if (remoteVersion.UpdatedAt > localVersion.UpdatedAt)
                        {
                            // Overwrite local changes, with the server's changes.
                            dbSet.Update(entry, GraphBehavior.SingleObject);
                        }
                    }
                    else
                    {
                        // Simply take the changes from the server, there are no valid local changes.
                        dbSet.Update(entry, GraphBehavior.SingleObject);
                    }
                }
            }
        }

        /// <summary>
        ///     Responds with a status message.
        /// </summary>
        /// <returns></returns>
        public async Task<string> PullAndPopulate(DateTimeOffset lastUpdate, Creds creds)
        {
            //Reds first since they are an independent set, and they do not require authentication.
            // 1.PlacementType
            // 2.Parameter
            // 3.Subsystem
            // 4.Placement_has_Parameter
            // 5.ControlTypes

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
                ParamsAtPlaces,
                Parameters,
                PlacementTypes,
                Subsystems,
                Devices,
                Relays,
                Sensors,
                ControlHistories,
                Controllables,
                Cycles,
                CropTypes,
                Greenhouses,
                SensorDatas
            };

            var placementType = await FetchTableAndDeserialise<PlacementType>(nameof(PlacementTypes));
            var parameters = await FetchTableAndDeserialise<Parameter>(nameof(Parameters));
            var subsystems = await FetchTableAndDeserialise<Subsystem>(nameof(Subsystems));
            var paramsAtPlaces = await FetchTableAndDeserialise<ParamAtPlace>(nameof(ParamsAtPlaces));
            var controlTypes = await FetchTableAndDeserialise<ControlType>(nameof(ControlTypes));


            //The user editable merging items. These also requres authentication.
            // 1.Greenhouse
            // 2.CropType
            // 3.Devices
            // 4.Cycle
            // 5.Sensors
            // 6.Relay
            // 7.

            var greenhouse = await FetchTableAndDeserialise<Greenhouse>(nameof(Greenhouses), creds);
            var cropTypes = await FetchTableAndDeserialise<CropType>(nameof(CropTypes), creds);
            var cycles = await FetchTableAndDeserialise<CropCycle>(nameof(Cycles), creds);
            var controllables = await FetchTableAndDeserialise<Controllable>(nameof(Controllables), creds);

            //TODO: waiting for API
            //var devices = await FetchTableAndDeserialise<Device>(nameof(Devices), creds);
            //var sensors = await FetchTableAndDeserialise<Sensor>(nameof(Sensors), creds);
            //var relay = await FetchTableAndDeserialise<Relay>(nameof(Relays), creds);

            // Finall the sliced items.
            // 1.ControlHistory
            // 2.SensorData

            //TODO: waiting for api and need to write requests.

            var responses = new[]
            {
                placementType, parameters, subsystems, paramsAtPlaces, controlTypes, greenhouse, cropTypes, cycles,
                controllables
            };

            var fails = responses.Where(r => r != null).ToList();
            return fails.Any() ? string.Join(", ", fails) : null;
        }
    }
}