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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using NetLib;
    using Newtonsoft.Json;
    using Util;

    public class MainDbContext : DbContext, IDataModel
    {
        /// <summary>
        ///     The Url of the web api that is used to fetch data.
        /// </summary>
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";

        private List<object> _tables;

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
        public DbSet<SensorHistory> SensorsHistory { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }

        /// <summary>
        ///     Downloads each table from the server and updates the data.
        /// </summary>
        /// <param name="lastUpdate">Last time update from server was called.</param>
        /// <param name="creds">Auth creds for the request.</param>
        /// <returns>Null on success, otherwise a descriptive error message.</returns>
        public async Task<string> UpdateFromServer(DateTimeOffset lastUpdate, Creds creds)
        {
            // No auth no post types:
            // 1. Parameters
            // 2. --------------------------People WRONG
            // 3. Placements
            // 4. RelayTypes
            // 5. SensorTypes
            // 6. Subsystems

            var responses = new List<string>();
            responses.Add(await DownloadDeserialiseTable<Parameter>(nameof(Parameters)));
            responses.Add(await DownloadDeserialiseTable<Placement>(nameof(Placements)));
            responses.Add(await DownloadDeserialiseTable<Subsystem>(nameof(Subsystems)));
            responses.Add(await DownloadDeserialiseTable<RelayType>(nameof(RelayTypes)));
            responses.Add(await DownloadDeserialiseTable<SensorType>(nameof(SensorTypes)));


            // Editable types that must be merged.
            // 1.CropCycles
            // 2.CropType (uniqley does not require auth on get).
            // 3.Devices
            // 4.Locations
            // 5.Relays
            // 6.

            responses.Add(await DownloadDeserialiseTable<Person>(nameof(People), creds));
            // Crop type is the only mergable that is no-auth.
            responses.Add(await DownloadDeserialiseTable<CropType>(nameof(CropTypes)));
            responses.Add(await DownloadDeserialiseTable<Location>(nameof(Locations), creds));
            responses.Add(await DownloadDeserialiseTable<CropCycle>(nameof(CropCycles), creds));
            responses.Add(await DownloadDeserialiseTable<Device>(nameof(Devices), creds));
            responses.Add(await DownloadDeserialiseTable<Relay>(nameof(Relays), creds));
            responses.Add(await DownloadDeserialiseTable<Sensor>(nameof(Sensors), creds));


            // Items that have to get got in time slices.
            // 1.RelayHistory
            // 2.SensorHistory

            var unixtime = lastUpdate == default(DateTimeOffset) ? 0 : lastUpdate.ToUnixTimeSeconds();

            responses.Add(
                await DownloadDeserialiseTable<SensorHistory>($"{nameof(SensorsHistory)}/{unixtime}/9001", creds));
            responses.Add(await DownloadDeserialiseTable<RelayHistory>($"{nameof(RelayHistory)}/{unixtime}/9001", creds));


            // Notify the app that the tables have been updated.
            await Messenger.Instance.UserTablesChanged.Invoke(null);

            await Messenger.Instance.HardwareTableChanged.Invoke(null);

            var fails = responses.Where(r => r != null).ToList();
            return fails.Any() ? string.Join(", ", fails) : null;
        }

        /// <summary>
        ///     A list of all the tables, used automatically select the correct table for updating.
        /// </summary>
        private void InitTableList()
        {
            if (null != _tables) return;

            _tables = new List<object>
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
                SensorsHistory,
                SensorTypes,
                Subsystems
            };
        }

        /// <summary>
        ///     Set filename for db.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=maindb.db");
        }

        /// <summary>
        ///     Configure the database model.
        /// </summary>
        /// <param name="mb"></param>
        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<RelayHistory>().HasKey(rh => new {rh.RelayID, rh.TimeStamp});
            mb.Entity<SensorHistory>().HasKey(sh => new {sh.SensorID, sh.TimeStamp});

            mb.Entity<Location>()
                .HasOne(l => l.Person)
                .WithMany(p => p.Locations)
                .HasForeignKey(l => l.PersonId).IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            // Skip the person init, it can't be editied on this side anyway.
            mb.Entity<Sensor>().Property(s => s.AlertHigh).IsRequired(false);
            mb.Entity<Sensor>().Property(s => s.AlertLow).IsRequired(false);

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

            mb.Entity<RelayHistory>().Ignore(rh => rh.Data);
            mb.Entity<SensorHistory>().Ignore(sh => sh.Data);
        }

        /// <summary>
        ///     Makes a webrequest to the API server to fetch a table.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the table.</typeparam>
        /// <param name="tableName">Name of the table to request.</param>
        /// <param name="cred">Credentials to be used to authenticate with the server. Only required for some types.</param>
        /// <returns>Null on success, otherwise an error message.</returns>
        private async Task<string> DownloadDeserialiseTable<TPoco>(string tableName, Creds cred = null)
            where TPoco : class
        {
            // Step 1: Request
            string response;
            if (cred == null)
                response = await Request.GetTable(ApiUrl, tableName);
            else
                response = await Request.GetTable(ApiUrl, tableName, cred);

            if (response.StartsWith("Error:"))
            {
                Debug.WriteLine($"Request failed: {tableName}, creds {null == cred}, {response}");

                return $"Request failed: {tableName}, {response}";
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
        ///     Figures out the real type of the table entitiy, performs checks for existing items and merges data where required.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the entity.</typeparam>
        /// <param name="updatesFromServer">The data recieved from the server.</param>
        /// <returns>Awaitable, the local database queries are done async.</returns>
        private async Task AddOrModify<TPoco>(List<TPoco> updatesFromServer)
            where TPoco : class
        {
            InitTableList();
            var dbSet = (DbSet<TPoco>) _tables.First(d => d is DbSet<TPoco>);
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
                else if (pocoType == typeof(SensorHistory))
                {
                    var hist = entry as SensorHistory;
                    var oldHist =
                        dbSet.OfType<SensorHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(d => d.SensorID == hist.SensorID && d.TimeStamp == hist.TimeStamp);
                    existing = oldHist as TPoco;
                    if (null != existing)
                    {
                        // They are the same primary key so merge them.
                        oldHist.DeserialiseData();
                        var merged = SensorHistory.Merge(hist, oldHist);
                        existing = merged as TPoco;
                    }

                    if (hist != null)
                    {
                        var id = hist.SensorID;
                        await Messenger.Instance.NewSensorDataPoint.Invoke(
                            hist.Data.Select(d => new Messenger.SensorReading(id, d.Value, d.TimeStamp, d.Duration)));
                    }
                }
                else if (pocoType == typeof(RelayHistory))
                {
                    var hist = entry as RelayHistory;
                    var oldHist =
                        dbSet.OfType<RelayHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(d => d.RelayID == hist.RelayID && d.TimeStamp == hist.TimeStamp);
                    existing = oldHist as TPoco;
                    if (null != existing)
                    {
                        // They are the same primary key so merge them.
                        oldHist.DeserialiseData();
                        var merged = DatabasePOCOs.User.RelayHistory.Merge(hist, oldHist);
                        existing = merged as TPoco;
                    }

                    if (hist != null)
                    {
                        var id = hist.RelayID;
                        await Messenger.Instance.NewRelayDataPoint.Invoke(
                            hist.Data.Select(d => new Messenger.RelayReading(id, d.State, d.TimeStamp, d.Duration)));
                    }
                }

                if (existing == null)
                {
                    dbSet.Add(entry);

                    // if (entry is BaseEntity)
                    // {
                    //     await Messenger.Instance.UserTablesChanged.Invoke("new");
                    // }
                    // else
                    // {
                    //     await Messenger.Instance.HardwareTableChanged.Invoke("new");
                    // }
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
                            // Overwrite local version, with the server's changes.
                            dbSet.Update(entry);
                            //await Messenger.Instance.UserTablesChanged.Invoke("update");
                        }
                        // We are not using this mode where ther server gets to override local changes. Far too confusing.
                        //else if (lastUpdated != default(DateTimeOffset) && remoteVersion.UpdatedAt > lastUpdated)
                        //{
                        //    // Overwrite local version with remote version that was modified since the last update.
                        //    // The local version is newer but we have decided to overwrite it 
                        //    dbSet.Update(entry);
                        //}
                    }
                    else if (typeof(TPoco) == typeof(SensorHistory))
                    {
                        // Minor hack, this is existing merged into entry.
                        var hist = entry as SensorHistory;
                        hist.SerialiseData();
                        dbSet.Update(entry);
                        // The messenger message is done earlier, no difference between new and update.
                    }
                    else if (typeof(TPoco) == typeof(RelayHistory))
                    {
                        var hist = entry as RelayHistory;
                        hist.SerialiseData();
                        dbSet.Update(entry);
                    }
                    else
                    {
                        // Simply take the changes from the server, there are no valid local changes.
                        dbSet.Update(entry);

                        //await Messenger.Instance.HardwareTableChanged.Invoke("new");
                    }
                }
            }
        }

        /// <summary>
        ///     Posts changes saved in the local DB (excluding histories) to the server.
        /// </summary>
        public async Task<List<string>> PostChanges()
        {
            var settings = new Settings();
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var lastDatabasePost = settings.LastDatabasePost;

            var postTime = DateTimeOffset.Now;
            var responses = new List<string>();
            // Simple tables that change:
            // CropCycle, Devices.
            responses.Add(await Post(Locations, nameof(Locations), lastDatabasePost, creds));
            responses.Add(await Post(CropCycles, nameof(CropCycles), lastDatabasePost, creds));
            responses.Add(await Post(Devices, nameof(Devices), lastDatabasePost, creds));
            responses.Add(await Post(Sensors, nameof(Sensors), lastDatabasePost, creds));
            responses.Add(await Post(Relays, nameof(Relays), lastDatabasePost, creds));

            // CropTypes is unique:
            var changedCropTypes = CropTypes.Where(c => c.CreatedAt > lastDatabasePost);

            if (changedCropTypes.Any())
            {
                var cropTypeData = JsonConvert.SerializeObject(changedCropTypes);
                responses.Add(await Request.PostTable(ApiUrl, nameof(CropTypes), cropTypeData, creds));
            }


            var errors = responses.Where(r => r != null).ToList();
            if (!errors.Any()) settings.LastDatabasePost = postTime;
            return errors;
        }


        /// <summary>
        ///     Posts all new history items since the last time data was posted.
        /// </summary>
        /// <returns></returns>
        public async Task<string> PostHistoryChanges()
        {
            var settings = new Settings();
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var lastSensorDataPost = settings.LastSensorDataPost;

            if (!SensorsHistory.Any()) return null;

            // Get the time just before we raid the database.
            var postTime = DateTimeOffset.Now;
            var needsPost = SensorsHistory.AsNoTracking().Where(s => s.TimeStamp > lastSensorDataPost).ToList();
            var deserialiseAndSlice = needsPost.Select(sensorHistory =>
            {
                // All data loaded from the DB must be deserialised to properly populate the poco object.
                sensorHistory.DeserialiseData();
                var endOfLastUpdateDay = (lastSensorDataPost + TimeSpan.FromDays(1)).Date;
                // If the last post was halfway though a day that day will need to be sliced.
                if (sensorHistory.TimeStamp > endOfLastUpdateDay) return sensorHistory;
                var slice = sensorHistory.Slice(lastSensorDataPost);
                return slice;
            });

            if (!deserialiseAndSlice.Any())
            {
                // Nothing to post so quit as a success.
                return null;
            }

            var json = JsonConvert.SerializeObject(deserialiseAndSlice);

            var result =
                await Request.PostTable(ApiUrl, nameof(SensorsHistory), json, creds);
            if (result == null)
            {
                // Only update last post if it was successfull.
                settings.LastSensorDataPost = postTime;
            }
            return result;
        }

        /// <summary>
        ///     Only supports tables that derive from BaseEntity and Croptype.
        /// </summary>
        /// <param name="table">The DBSet object for the table.</param>
        /// <param name="tableName">The name of the table in the API .</param>
        /// <param name="lastPost">The last time the table was synced.</param>
        /// <param name="creds">Authentication credentials.</param>
        /// <returns>Null on success otherwise an error message.</returns>
        private async Task<string> Post(IQueryable<BaseEntity> table, string tableName, DateTimeOffset lastPost,
            Creds creds)
        {
            var edited = table
                .Where(t => t.UpdatedAt > lastPost)
                .ToList();

            if (!edited.Any()) return null;

            var data = JsonConvert.SerializeObject(edited, Formatting.None);
            var req = await Request.PostTable(ApiUrl, tableName, data, creds);
            return req;
        }
    }
}