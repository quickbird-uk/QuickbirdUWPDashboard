namespace Agronomist.Util
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
            var treeQueryWrappedInAnContinuation = AttachContinuationsAndSwapLastTask(TreeQueryAsync);
            stopwatchA.Stop();

            var stopwatchB = Stopwatch.StartNew();
            // await the continuation, then the treeQuery wrapped in (and executed by) the continuation.
            // technically we could await once and then take the .Result, it is the same thing.
            var userData = await await treeQueryWrappedInAnContinuation;
            stopwatchB.Stop();

            Debug.WriteLine(
                $"Querying data-tree took {stopwatchB.ElapsedMilliseconds} milliseconds, (setup: {stopwatchA.ElapsedMilliseconds}ms).");

            return userData;
        }

        /// <summary>
        ///     Gets a large tree of all of the data the UI uses except for live and historical readings.
        /// </summary>
        /// <returns>Non-readings tree of data used bvy the UI.</returns>
        private static async Task<List<CropCycle>> TreeQueryAsync()
        {
            using (var db = new MainDbContext())
            {
                return
                    await
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
                            // ConfigureAwait false with cause this to run on the threadpool.
                            .ToListAsync().ConfigureAwait(false);
            }
        }


        public async Task<List<KeyValuePair<Guid, List<SensorHistory>>>> GetDataReadingsAsync(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            // Wrap in a function, attach continuation and set _lastTask before awaiting.
            var queryContinuation =
                AttachContinuationsAndSwapLastTask(() => QueryDataReadingAsync(sensorIDs, start, end));
            return await await queryContinuation;
        }

        private async Task<List<KeyValuePair<Guid, List<SensorHistory>>>> QueryDataReadingAsync(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            var trueStartDate = start.AddDays(-1);
            //Because of the way we put timestamps on days, we need to subtracks one

            List<SensorHistory> readings;
            using (var db = new MainDbContext())
            {
                readings = await db.SensorsHistory
                    .Where(
                        sh => sh.TimeStamp > trueStartDate
                              && sh.TimeStamp < end && sensorIDs.Contains(sh.SensorID))
                    .AsNoTracking()
                    .ToListAsync()
                    // ConfigureAwait false with cause this to run on the threadpool.
                    .ConfigureAwait(false);
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
        /// Updates the database from the cloud server.
        /// </summary>
        /// <param name="lastUpdate"></param>
        /// <param name="creds"></param>
        /// <returns>A compilation of errors.</returns>
        public async Task<List<string>> GetUpdatesFromServerAsync(DateTimeOffset lastUpdate, Creds creds)
        {
            var cont = AttachContinuationsAndSwapLastTask(() => UpdateFromServerAsync(lastUpdate, creds));
            return await await cont;
        }

        private async Task<List<string>> UpdateFromServerAsync(DateTimeOffset lastUpdate, Creds creds)
        {
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

                responses.Add(await DownloadDeserialiseTable<Parameter>(nameof(db.Parameters), tableList));
                responses.Add(await DownloadDeserialiseTable<Placement>(nameof(db.Placements), tableList));
                responses.Add(await DownloadDeserialiseTable<Subsystem>(nameof(db.Subsystems), tableList));
                responses.Add(await DownloadDeserialiseTable<RelayType>(nameof(db.RelayTypes), tableList));
                responses.Add(await DownloadDeserialiseTable<SensorType>(nameof(db.SensorTypes), tableList));

                if(responses.Any(r=>r!=null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                await db.SaveChangesAsync().ConfigureAwait(false);

                // Editable types that must be merged.
                // 1.CropCycles
                // 2.CropType (uniqley does not require auth on get).
                // 3.Devices
                // 4.Locations
                // 5.Relays
                // 6.

                responses.Add(await DownloadDeserialiseTable<Person>(nameof(db.People), tableList, creds));
                // Crop type is the only mergable that is no-auth.
                responses.Add(await DownloadDeserialiseTable<CropType>(nameof(db.CropTypes), tableList));
                responses.Add(await DownloadDeserialiseTable<Location>(nameof(db.Locations), tableList, creds));
                responses.Add(await DownloadDeserialiseTable<CropCycle>(nameof(db.CropCycles), tableList, creds));
                responses.Add(await DownloadDeserialiseTable<Device>(nameof(db.Devices), tableList, creds));
                responses.Add(await DownloadDeserialiseTable<Relay>(nameof(db.Relays), tableList, creds));
                responses.Add(await DownloadDeserialiseTable<Sensor>(nameof(db.Sensors), tableList, creds));

                if (responses.Any(r => r != null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                await db.SaveChangesAsync().ConfigureAwait(false);

                // Items that have to get got in time slices.
                // 1.RelayHistory
                // 2.SensorHistory

                var unixtime = lastUpdate == default(DateTimeOffset) ? 0 : lastUpdate.ToUnixTimeSeconds();

                responses.Add(
                    await
                        DownloadDeserialiseTable<SensorHistory>($"{nameof(db.SensorsHistory)}/{unixtime}/9001",
                            tableList, creds));
                responses.Add(
                    await
                        DownloadDeserialiseTable<RelayHistory>($"{nameof(db.RelayHistory)}/{unixtime}/9001", tableList,
                            creds));

                if (responses.Any(r => r != null))
                {
                    return responses.Where(r => r != null).ToList();
                }
                await db.SaveChangesAsync().ConfigureAwait(false);
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
            await AddOrModify(updatesFromServer, tableList);

            return null;
        }

        /// <summary>
        ///     Figures out the real type of the table entitiy, performs checks for existing items and merges data where required.
        /// </summary>
        /// <typeparam name="TPoco">The POCO type of the entity.</typeparam>
        /// <param name="updatesFromServer">The data recieved from the server.</param>
        /// <returns>Awaitable, the local database queries are done async.</returns>
        private async Task AddOrModify<TPoco>(List<TPoco> updatesFromServer, List<object> _tables)
            where TPoco : class
        {
            var dbSet = (DbSet<TPoco>) _tables.First(d => d is DbSet<TPoco>);
            var pocoType = typeof(TPoco);
            foreach (var remote in updatesFromServer)
            {
                TPoco local = null;
                TPoco merged = null;
                if (pocoType.GetInterfaces().Contains(typeof(IHasId)))
                {
                    local =
                        await
                            dbSet.Select(a => a)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => ((IHasId) d).ID == ((IHasId) remote).ID);
                }
                else if (pocoType.GetInterfaces().Contains(typeof(IHasGuid)))
                {
                    local =
                        await
                            dbSet.Select(a => a)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(d => ((IHasGuid) d).ID == ((IHasGuid) remote).ID);
                }
                else if (pocoType == typeof(CropType))
                {
                    var x = remote as CropType;
                    local = dbSet.OfType<CropType>().AsNoTracking().FirstOrDefault(d => d.Name == x.Name) as TPoco;
                }
                else if (pocoType == typeof(SensorHistory))
                {
                    var remoteSH = remote as SensorHistory;
                    var localSH =
                        dbSet.OfType<SensorHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(d => d.SensorID == remoteSH.SensorID && d.TimeStamp == remoteSH.TimeStamp);
                    local = localSH as TPoco;
                    if (null != local)
                    {
                        // They are the same primary key so merge them.
                        localSH.DeserialiseData();
                        var mergedSH = SensorHistory.Merge(remoteSH, localSH);
                        mergedSH.SerialiseData();
                        merged = mergedSH as TPoco;
                    }
                    else
                    {
                        remoteSH.SerialiseData();
                    }

                    if (remoteSH != null)
                    {
                        var id = remoteSH.SensorID;
                        await Messenger.Instance.NewSensorDataPoint.Invoke(
                            remoteSH.Data.Select(d => new Messenger.SensorReading(id, d.Value, d.TimeStamp, d.Duration)));
                    }
                }
                else if (pocoType == typeof(RelayHistory))
                {
                    var remoteRH = remote as RelayHistory;
                    var oldHist =
                        dbSet.OfType<RelayHistory>()
                            .AsNoTracking()
                            .FirstOrDefault(d => d.RelayID == remoteRH.RelayID && d.TimeStamp == remoteRH.TimeStamp);
                    local = oldHist as TPoco;
                    if (null != local)
                    {
                        // They are the same primary key so merge them.
                        oldHist.DeserialiseData();
                        var mergedRH = RelayHistory.Merge(remoteRH, oldHist);
                        mergedRH.SerialiseData();
                        merged = mergedRH as TPoco;
                    }
                    else
                    {
                        remoteRH.SerialiseData();
                    }

                    if (remoteRH != null)
                    {
                        var id = remoteRH.RelayID;
                        await Messenger.Instance.NewRelayDataPoint.Invoke(
                            remoteRH.Data.Select(d => new Messenger.RelayReading(id, d.State, d.TimeStamp, d.Duration)));
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
                        dbSet.Update(merged);
                        // The messenger message is done earlier, no difference between new and update.
                    }
                    else if (typeof(TPoco) == typeof(RelayHistory))
                    {
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


        /// <summary>
        ///     Posts changes saved in the local DB (excluding histories) to the server.
        /// </summary>
        private async Task<List<string>> PostChanges()
        {
            var settings = Settings.Instance;
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
        private async Task<string> PostHistoryChanges()
        {
            var settings = Settings.Instance;
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