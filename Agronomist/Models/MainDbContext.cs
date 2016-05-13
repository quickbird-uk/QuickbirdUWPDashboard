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

        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<RelayHistory> RelayHistory { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<RelayType> RelayTypes { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorHistory> SensorHistory { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=maindb.db");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<RelayHistory>().HasKey(rh => new {rh.RelayID, rh.TimeStamp});
            mb.Entity<SensorHistory>().HasKey(sh => new { sh.SensorID, sh.TimeStamp });

            mb.Entity<Location>()
                .HasOne(l => l.Person)
                .WithMany(p => p.Locations)
                .HasForeignKey(l => l.PersonId).IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            // Skip the person init, it can't be editied on this side anyway.

            mb.Entity<Location>()
                .HasMany(loc => loc.SensorHistory)
                .WithOne(sh => sh.Location)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            mb.Entity<CropType>()
                .HasKey(ct => ct.Name);

            mb.Entity<CropType>()
                .Property(ct => ct.Name)
                .ValueGeneratedNever();

            mb.Entity<CropType>()
                .HasMany(ct => ct.CropCycles)
                .WithOne(cc => cc.CropType).IsRequired()
                .HasForeignKey(cc => cc.CropTypeName);         
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
            _dbos = new List<object>
            {
                CropCycles,
                CropTypes,
                Devices,
                Locations,
                Parameters,
                People,
                Placements,
                RelayHistory,
                Relays,
                RelayTypes,
                Sensors,
                SensorHistory,
                SensorTypes,
                Subsystems
            };

            // No auth no post types:
            // 1. Parameters
            // 2. People
            // 3. Placements
            // 4. RelayTypes
            // 5. SensorTypes
            // 6. Subsystems

            var responses = new List<string>();
            responses.Add(await FetchTableAndDeserialise<Parameter>(nameof(Parameters)));
            responses.Add(await FetchTableAndDeserialise<Person>(nameof(People)));
            responses.Add(await FetchTableAndDeserialise<Placement>(nameof(Placements)));
            responses.Add(await FetchTableAndDeserialise<RelayType>(nameof(RelayTypes)));
            responses.Add(await FetchTableAndDeserialise<SensorType>(nameof(SensorTypes)));
            responses.Add(await FetchTableAndDeserialise<Subsystem>(nameof(Subsystems)));


            // Editable types that must be merged.
            // 1.CropCycles
            // 2.CropType (uniqley does not require auth on get).
            // 3.Devices
            // 4.Locations
            // 5.Relays
            // 6.Sensors

            responses.Add(await FetchTableAndDeserialise<CropCycle>(nameof(CropCycles), creds));
            responses.Add(await FetchTableAndDeserialise<CropType>(nameof(CropTypes), creds));
            responses.Add(await FetchTableAndDeserialise<Device>(nameof(Devices), creds));
            responses.Add(await FetchTableAndDeserialise<Location>(nameof(Locations), creds));
            responses.Add(await FetchTableAndDeserialise<Relay>(nameof(Relays), creds));
            responses.Add(await FetchTableAndDeserialise<Sensor>(nameof(Sensors), creds));



            // Items that have to get got in time slices.
            // 1.RelayHistory
            // 2.SensorHistory

            //TODO: waiting for api and need to write requests.


            var fails = responses.Where(r => r != null).ToList();
            return fails.Any() ? string.Join(", ", fails) : null;
        }
    }
}