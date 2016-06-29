namespace Quickbird.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DbStructure;
    using DbStructure.Global;
    using DbStructure.User;
    using Internet;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     The public methods in theis helper are meant to be run on the UI thread - this is required to force the methods to
    ///     be executed consecutively and prevent overlap.
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        ///     The Url of the web api that is used to fetch data.
        /// </summary>
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";

        /// <summary>
        ///     An complete task that can be have ContinueWith() called on it.
        ///     Used to queue database tasks to make sure one completes before another starts.
        /// </summary>
        private Task _lastTask = Task.CompletedTask;

        private DatabaseHelper()
        {
        }

        /// <summary>
        ///     Singleton instance accessor.
        /// </summary>
        public static DatabaseHelper Instance { get; } = new DatabaseHelper();

        /// <summary>
        ///     The method should be executed on the UI thread, which means it should be called before any awaits, before the the
        ///     method returns.
        /// </summary>
        private Task<T> AttachContinuationsAndSwapLastTask<T>(Func<T> workForNextTask)
        {
            var contTask = _lastTask.ContinueWith(_ => workForNextTask());
            _lastTask = contTask;
            return contTask;
        }

        /// <summary>
        ///     Gets all of the non-reading data that the UI uses as a big tree starting from each crop cycle.
        /// </summary>
        /// <returns>CropCycle objects with all the non-reading data the UI uses included.</returns>
        public async Task<List<CropCycle>> GetDataTreeAsync()
        {
            var stopwatchA = Stopwatch.StartNew();
            // Swap out the continuation task before any awaits are called.
            // We want to replace the _lastTask field before this method returns.
            // We cannot do this after an await because the method may return on the await statement.
            var treeQueryWrappedInAnContinuation = AttachContinuationsAndSwapLastTask(() => Task.Run(() => TreeQuery()));
            stopwatchA.Stop();

            var stopwatchB = Stopwatch.StartNew();
            // await the continuation, then the treeQuery wrapped in (and executed by) the continuation.
            // technically we could await once and then take the .Result, it is the same thing.
            var userData = await await treeQueryWrappedInAnContinuation.ConfigureAwait(false);
            stopwatchB.Stop();

            Debug.WriteLine(
                $"Querying data-tree took {stopwatchB.ElapsedMilliseconds} milliseconds, (setup: {stopwatchA.ElapsedMilliseconds}ms).");

            return userData;
        }

        /// <summary>
        ///     Gets a large tree of all of the data the UI uses except for live and historical readings.
        ///     Run on threadpool because SQLite doesn't do async IO.
        /// </summary>
        /// <returns>Non-readings tree of data used bvy the UI.</returns>
        private List<CropCycle> TreeQuery()
        {
            using (var db = new MainDbContext())
            {
                return
                    db.CropCycles.Include(cc => cc.Location)
                        .Include(cc => cc.CropType)
                        .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Param)
                        .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Place)
                        .AsNoTracking()
                        .ToList();
            }
        }

        public async Task<List<KeyValuePair<Guid, List<SensorHistory>>>> GetDataReadingsAsync(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            // Wrap in a function, attach continuation and set _lastTask before awaiting.
            var queryContinuation =
                AttachContinuationsAndSwapLastTask(() => Task.Run(() => QueryDataReading(sensorIDs, start, end)));
            return await await queryContinuation.ConfigureAwait(false);
        }

        private List<KeyValuePair<Guid, List<SensorHistory>>> QueryDataReading(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            var trueStartDate = start.AddDays(-1);
            //Because of the way we put timestamps on days, we need to subtracks one

            List<SensorHistory> readings;
            using (var db = new MainDbContext())
            {
                readings = db.SensorsHistory
                    .Where(
                        sh => sh.TimeStamp > trueStartDate
                              && sh.TimeStamp < end && sensorIDs.Contains(sh.SensorID))
                    .AsNoTracking()
                    .ToList();
            }

            var result = new List<KeyValuePair<Guid, List<SensorHistory>>>();

            foreach (var sh in readings)
            {
                var sensorCollection = result.FirstOrDefault(kv => kv.Key == sh.SensorID);
                //Check if this collection already exists
                if (sensorCollection.Key == sh.SensorID == false)
                {
                    sensorCollection = new KeyValuePair<Guid, List<SensorHistory>>(sh.SensorID,
                        new List<SensorHistory>());
                    result.Add(sensorCollection);
                }
                sensorCollection.Value.Add(sh);
            }

            return result;
        }

        /// <summary>
        ///     Updates the database from the cloud server.
        /// </summary>
        /// <returns>A compilation of errors.</returns>
        public async Task<List<string>> GetUpdatesFromServerAsync()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(UpdateFromServerAsync));
            var updatesFromServerAsync = await await cont.ConfigureAwait(false);
            await Messenger.Instance.TablesChanged.Invoke(null);
            return updatesFromServerAsync;
        }

        private async Task<List<string>> UpdateFromServerAsync()
        {
            var settings = Settings.Instance;
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var lastUpdate = settings.LastDatabaseUpdate;
            var now = DateTimeOffset.Now;

            var responses = new List<string>();

            using (var db = new MainDbContext())
            {
                var tableList = new List<object>
                {
                    db.CropCycles,
                    db.CropTypes,
                    db.Devices,
                    db.Locations,
                    db.Parameters,
                    db.People,
                    db.Placements,
                    db.RelayHistory,
                    db.Relays,
                    db.RelayTypes,
                    db.Sensors,
                    db.SensorsHistory,
                    db.SensorTypes,
                    db.Subsystems
                };

                // No auth no post types:
                // 1. Parameters
                // 2. --------------------------People WRONG
                // 3. Placements
                // 4. RelayTypes
                // 5. SensorTypes
                // 6. Subsystems


                // This might look like a good idea but it wont work, the dbcontext is not threadsafe.
                // The download part of that method could be done in paralell, but the rest need to be done in order.
                //var tasks = new[]
                //{
                //    Task.Run(() => DownloadDeserialiseTable<Parameter>(nameof(db.Parameters), tableList)),
                //    Task.Run(() => DownloadDeserialiseTable<Placement>(nameof(db.Placements), tableList)),
                //    Task.Run(() => DownloadDeserialiseTable<Subsystem>(nameof(db.Subsystems), tableList)),
                //    Task.Run(() => DownloadDeserialiseTable<RelayType>(nameof(db.RelayTypes), tableList)),
                //    Task.Run(() => DownloadDeserialiseTable<SensorType>(nameof(db.SensorTypes), tableList))
                //};

                //await Task.WhenAll(tasks);

                // Setting configure await to false allows all of this method to be run on the threadpool.
                // Without setting it false the continuation would be posted onto the SynchronisationContext, which is the UI.
                responses.Add(
                    await DownloadDeserialiseTable<Parameter>(nameof(db.Parameters), tableList).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<Placement>(nameof(db.Placements), tableList).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<Subsystem>(nameof(db.Subsystems), tableList).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<RelayType>(nameof(db.RelayTypes), tableList).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<SensorType>(nameof(db.SensorTypes), tableList).ConfigureAwait(false));

                if (responses.Any(r => r != null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                db.SaveChanges();

                // Editable types that must be merged.
                // 1.CropCycles
                // 2.CropType (uniqley does not require auth on get).
                // 3.Devices
                // 4.Locations
                // 5.Relays
                // 6.

                responses.Add(
                    await DownloadDeserialiseTable<Person>(nameof(db.People), tableList, creds).ConfigureAwait(false));
                // Crop type is the only mergable that is no-auth.
                responses.Add(
                    await DownloadDeserialiseTable<CropType>(nameof(db.CropTypes), tableList).ConfigureAwait(false));
                responses.Add(
                    await
                        DownloadDeserialiseTable<Location>(nameof(db.Locations), tableList, creds).ConfigureAwait(false));
                responses.Add(
                    await
                        DownloadDeserialiseTable<CropCycle>(nameof(db.CropCycles), tableList, creds)
                            .ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<Device>(nameof(db.Devices), tableList, creds).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<Relay>(nameof(db.Relays), tableList, creds).ConfigureAwait(false));
                responses.Add(
                    await DownloadDeserialiseTable<Sensor>(nameof(db.Sensors), tableList, creds).ConfigureAwait(false));

                if (responses.Any(r => r != null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                db.SaveChanges();

                // Items that have to get got in time slices.
                // 1.RelayHistory
                // 2.SensorHistory

                var unixtime = lastUpdate == default(DateTimeOffset) ? 0 : lastUpdate.ToUnixTimeSeconds();

                responses.Add(
                    await
                        DownloadDeserialiseTable<SensorHistory>($"{nameof(db.SensorsHistory)}/{unixtime}/9001",
                            tableList, creds).ConfigureAwait(false));
                responses.Add(
                    await
                        DownloadDeserialiseTable<RelayHistory>($"{nameof(db.RelayHistory)}/{unixtime}/9001", tableList,
                            creds).ConfigureAwait(false));

                if (responses.Any(r => r != null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                db.SaveChanges();
            }

            responses = responses.Where(r => r != null).ToList();

            if (!responses.Any())
            {
                settings.LastDatabaseUpdate = now;
            }

            return responses;
        }

        /// <summary>
        ///     Makes a webrequest to the API server to fetch a table.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the table.</typeparam>
        /// <param name="tableName">Name of the table to request.</param>
        /// <param name="tableList">A collection of dbset </param>
        /// <param name="cred">Credentials to be used to authenticate with the server. Only required for some types.</param>
        /// <returns>Null on success, otherwise an error message.</returns>
        private async Task<string> DownloadDeserialiseTable<TPoco>(string tableName, List<object> tableList,
            Creds cred = null)
            where TPoco : class
        {
            // Step 1: Request
            string response;
            if (cred == null)
                response = await Request.GetTable(ApiUrl, tableName).ConfigureAwait(false);
            else
                response = await Request.GetTable(ApiUrl, tableName, cred).ConfigureAwait(false);

            if (response.StartsWith("Error:"))
            {
                Debug.WriteLine($"Request failed: {tableName}, creds {null == cred}, {response}");

                return $"Request failed: {tableName}, {response}";
            }


            // Step 2: Deserialise
            List<TPoco> updatesFromServer;
            try
            {
                updatesFromServer =
                    await Task.Run(() => JsonConvert.DeserializeObject<List<TPoco>>(response)).ConfigureAwait(false);
            }
            catch (JsonSerializationException e)
            {
                Debug.WriteLine($"Desserialise falied on response for {tableName}, creds {null == cred}.");
                Debug.WriteLine(e);
                return "Unable to deserialise.";
            }

            // Step 3: Merge
            // Get the DbSet that this request should be inserted into.
            await AddOrModify(updatesFromServer, tableList).ConfigureAwait(false);

            return null;
        }

        /// <summary>
        ///     Figures out the real type of the table entitiy, performs checks for existing items and merges data where required.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the entity.</typeparam>
        /// <param name="updatesFromServer">The data recieved from the server.</param>
        /// <param name="tables">List of the DbSet table objects from the context that the updates could belong to.</param>
        /// <returns>Awaitable, the local database queries are done async.</returns>
        private async Task AddOrModify<TPoco>(List<TPoco> updatesFromServer, List<object> tables)
            where TPoco : class
        {
            var dbSet = (DbSet<TPoco>) tables.First(d => d is DbSet<TPoco>);
            var pocoType = typeof(TPoco);
            foreach (var remote in updatesFromServer)
            {
                TPoco local = null;
                TPoco merged = null;
                if (pocoType.GetInterfaces().Contains(typeof(IHasId)))
                {
                    local =
                        dbSet.Select(a => a)
                            .AsNoTracking()
                            .FirstOrDefault(d => ((IHasId) d).ID == ((IHasId) remote).ID);
                }
                else if (pocoType.GetInterfaces().Contains(typeof(IHasGuid)))
                {
                    local =
                        dbSet.Select(a => a)
                            .AsNoTracking()
                            .FirstOrDefault(d => ((IHasGuid) d).ID == ((IHasGuid) remote).ID);
                }
                else if (pocoType == typeof(CropType))
                {
                    var x = remote as CropType;
                    local = dbSet.OfType<CropType>().AsNoTracking().FirstOrDefault(d => d.Name == x.Name) as TPoco;
                }
                else if (pocoType == typeof(SensorHistory))
                {
                    var remoteSenHist = remote as SensorHistory;
                    var localSenHist =
                        dbSet.OfType<SensorHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(
                                d => d.SensorID == remoteSenHist.SensorID && d.TimeStamp == remoteSenHist.TimeStamp);
                    local = localSenHist as TPoco;
                    if (null != local)
                    {
                        // They are the same primary key so merge them.
                        localSenHist.DeserialiseData();
                        var mergedSenHist = SensorHistory.Merge(remoteSenHist, localSenHist);
                        mergedSenHist.SerialiseData();
                        merged = mergedSenHist as TPoco;
                    }
                    else
                    {
                        Debug.Assert(remoteSenHist != null, "remoteSenHist != null, poco type detection failed.");
                        remoteSenHist.SerialiseData();
                    }

                    if (remoteSenHist != null)
                    {
                        var id = remoteSenHist.SensorID;
                        await Messenger.Instance.NewSensorDataPoint.Invoke(
                                remoteSenHist.Data.Select(
                                    d => new Messenger.SensorReading(id, d.Value, d.TimeStamp, d.Duration)))
                            .ConfigureAwait(false);
                    }
                }
                else if (pocoType == typeof(RelayHistory))
                {
                    var remoteRelayHist = remote as RelayHistory;
                    var oldHist =
                        dbSet.OfType<RelayHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(
                                d => d.RelayID == remoteRelayHist.RelayID && d.TimeStamp == remoteRelayHist.TimeStamp);
                    local = oldHist as TPoco;
                    if (null != local)
                    {
                        // They are the same primary key so merge them.
                        oldHist.DeserialiseData();
                        var mergedRelayHist = RelayHistory.Merge(remoteRelayHist, oldHist);
                        mergedRelayHist.SerialiseData();
                        merged = mergedRelayHist as TPoco;
                    }
                    else
                    {
                        Debug.Assert(remoteRelayHist != null, "remoteRelayHist != null, poco type detection failed.");
                        remoteRelayHist.SerialiseData();
                    }

                    if (remoteRelayHist != null)
                    {
                        var id = remoteRelayHist.RelayID;
                        await Messenger.Instance.NewRelayDataPoint.Invoke(
                                remoteRelayHist.Data.Select(
                                    d => new Messenger.RelayReading(id, d.State, d.TimeStamp, d.Duration)))
                            .ConfigureAwait(false);
                    }
                }

                //Whatever it is, jsut add record to the DB 
                if (local == null)
                {
                    dbSet.Add(remote);
                }
                else
                {
                    //User Tables
                    if (remote is BaseEntity && local is BaseEntity)
                    {
                        // These types allow local changes. Check date and don't overwrite unless the server has changed.
                        var remoteVersion = remote as BaseEntity;
                        var localVersion = local as BaseEntity;

                        if (remoteVersion.UpdatedAt > localVersion.UpdatedAt)
                        {
                            // Overwrite local version, with the server's changes.
                            dbSet.Update(remote);
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
                        Debug.Assert(merged != null, "merged != null, poco type detection failed.");
                        dbSet.Update(merged);
                        // The messenger message is done earlier, no difference between new and update.
                    }
                    else if (typeof(TPoco) == typeof(RelayHistory))
                    {
                        Debug.Assert(merged != null, "merged != null, poco type detection failed.");
                        dbSet.Update(merged);
                    }
                    //RED - Global read-only tables
                    else
                    {
                        // Simply take the changes from the server, there are no valid local changes.
                        dbSet.Update(remote);
                        //await Messenger.Instance.HardwareTableChanged.Invoke("new");
                    }
                }
            }
        }

        public async Task<List<string>> PostUpdatesAsync()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(PostUpdateToServerAsync));
            return await await cont.ConfigureAwait(false);
        }

        public async Task<string> PostHistoryAsync()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(PostHistoryChangesAsync));
            return await await cont.ConfigureAwait(false);
        }

        /// <summary>
        ///     Posts changes saved in the local DB (excluding histories) to the server.
        /// </summary>
        private async Task<List<string>> PostUpdateToServerAsync()
        {
            var settings = Settings.Instance;
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var lastDatabasePost = settings.LastDatabasePost;

            var postTime = DateTimeOffset.Now;
            var responses = new List<string>();
            // Simple tables that change:
            // CropCycle, Devices.
            using (var db = new MainDbContext())
            {
                responses.Add(
                    await PostAsync(db.Locations, nameof(db.Locations), lastDatabasePost, creds).ConfigureAwait(false));

                // CropTypes is unique:
                var changedCropTypes = db.CropTypes.Where(c => c.CreatedAt > lastDatabasePost);

                if (changedCropTypes.Any())
                {
                    var cropTypeData = JsonConvert.SerializeObject(changedCropTypes);
                    responses.Add(
                        await Request.PostTable(ApiUrl, nameof(db.CropTypes), cropTypeData, creds).ConfigureAwait(false));
                }

                responses.Add(
                    await PostAsync(db.CropCycles, nameof(db.CropCycles), lastDatabasePost, creds).ConfigureAwait(false));
                responses.Add(
                    await PostAsync(db.Devices, nameof(db.Devices), lastDatabasePost, creds).ConfigureAwait(false));
                responses.Add(
                    await PostAsync(db.Sensors, nameof(db.Sensors), lastDatabasePost, creds).ConfigureAwait(false));
                responses.Add(
                    await PostAsync(db.Relays, nameof(db.Relays), lastDatabasePost, creds).ConfigureAwait(false));


            }

            var errors = responses.Where(r => r != null).ToList();
            if (!errors.Any()) settings.LastDatabasePost = postTime;
            return errors;
        }

        /// <summary>
        ///     Posts all new history items since the last time data was posted.
        /// </summary>
        /// <returns></returns>
        private async Task<string> PostHistoryChangesAsync()
        {
            var settings = Settings.Instance;
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var lastSensorDataPost = settings.LastSensorDataPost;
            string result;
            using (var db = new MainDbContext())
            {
                if (!db.SensorsHistory.Any()) return null;

                // Get the time just before we raid the database.
                var postTime = DateTimeOffset.Now;
                var needsPost = db.SensorsHistory.AsNoTracking().Where(s => s.TimeStamp > lastSensorDataPost).ToList();


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

                result = await Request.PostTable(ApiUrl, nameof(db.SensorsHistory), json, creds).ConfigureAwait(false);
                if (result == null)
                {
                    // Only update last post if it was successfull.
                    settings.LastSensorDataPost = postTime;
                }
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
        private async Task<string> PostAsync(IQueryable<BaseEntity> table, string tableName, DateTimeOffset lastPost,
            Creds creds)
        {
            var edited = table
                .Where(t => t.UpdatedAt > lastPost)
                .ToList();

            if (!edited.Any()) return null;

            var data = JsonConvert.SerializeObject(edited, Formatting.None);
            var req = await Request.PostTable(ApiUrl, tableName, data, creds).ConfigureAwait(false);
            return req;
        }
    }
}