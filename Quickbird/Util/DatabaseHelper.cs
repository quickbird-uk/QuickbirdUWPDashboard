namespace Quickbird.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using DbStructure;
    using DbStructure.User;
    using Internet;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json;

    /// <summary>Methods to access and update the database. </summary>
    public class DatabaseHelper
    {
        /// <summary>The Url of the web api that is used to fetch data.</summary>
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";

        private const int MaximumDaysToDownload = 5;

        /// <summary>An complete task that can be have ContinueWith() called on it. Used to queue database
        /// tasks to make sure one completes before another starts.</summary>
        private Task _lastTask = Task.CompletedTask;

        private DatabaseHelper() { }

        /// <summary>Singleton instance accessor.</summary>
        public static DatabaseHelper Instance { get; } = new DatabaseHelper();

        /// <summary>Gets all of the non-reading data that the UI uses as a big tree starting from each crop
        /// cycle.</summary>
        /// <returns>CropCycle objects with all the non-reading data the UI uses included.</returns>
        public async Task<List<CropCycle>> GetDataTreeAsyncQueued()
        {
            var stopwatchA = Stopwatch.StartNew();
            // Swap out the continuation task before any awaits are called.
            // We want to replace the _lastTask field before this method returns.
            // We cannot do this after an await because the method may return on the await statement.
            var treeQueryWrappedInAnContinuation =
                AttachContinuationsAndSwapLastTask(() => Task.Run(() => GetDataTree()));
            stopwatchA.Stop();

            var stopwatchB = Stopwatch.StartNew();
            // await the continuation, then the treeQuery wrapped in (and executed by) the continuation.
            // technically we could await once and then take the .Result, it is the same thing.
            var userData = await await treeQueryWrappedInAnContinuation.ConfigureAwait(false);
            stopwatchB.Stop();

            Debug.WriteLine($"Querying data-tree took {stopwatchB.ElapsedMilliseconds} milliseconds, " +
                            $"(setup: {stopwatchA.ElapsedMilliseconds}ms).");

            return userData;
        }

        /// <summary>Requires UI thread. Syncs databse data with the server.</summary>
        /// <remarks>This method runs on the UI thread, the queued methods need it, they hand off work to the
        /// threadpool as appropriate.</remarks>
        public async Task SyncWithServerAsyncQueued()
        {
            var updateErrors = await GetRequestUpdateAsyncQueued();
            if (updateErrors?.Any() ?? false)
            {
                Debug.WriteLine(updateErrors);
                return;
            }

            var updateHistErrors = await GetRequestSensorHistoryAsyncQueued();
            if (updateHistErrors?.Any() ?? false)
            {
                Debug.WriteLine(updateHistErrors);
                return;
            }

            var postErrors = await PostRequestUpdatesAsyncQueued();
            if (postErrors?.Any() ?? false)
            {
                Debug.WriteLine(string.Join(",", postErrors));
                return;
            }

            var postHistErrors = await PostRequestHistoryAsyncQueued();
            if (postHistErrors?.Any() ?? false)
            {
                Debug.WriteLine(postHistErrors);
            }
        }

        /// <summary>Figures out the real type of the table entitiy, performs checks for existing items and
        /// merges data where required.</summary>
        /// <typeparam name="TPoco">The POCO type of the entity.</typeparam>
        /// <param name="updatesFromServer">The data recieved from the server.</param>
        /// <param name="dbSet">The actual databse table.</param>
        /// <returns>Awaitable, the local database queries are done async.</returns>
        private void AddOrModify<TPoco>(List<TPoco> updatesFromServer, DbSet<TPoco> dbSet) where TPoco : class
        {
            var pocoType = typeof(TPoco);
            foreach (var remote in updatesFromServer)
            {
                TPoco local = null;
                if (pocoType.GetInterfaces().Contains(typeof(IHasId)))
                {
                    local =
                        dbSet.Select(a => a).AsNoTracking().FirstOrDefault(d => ((IHasId) d).ID == ((IHasId) remote).ID);
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


        /// <summary>The method should be executed on the UI thread, which means it should be called before any
        /// awaits, before the the method returns.</summary>
        private Task<T> AttachContinuationsAndSwapLastTask<T>(Func<T> workForNextTask)
        {
            var contTask = _lastTask.ContinueWith(_ => workForNextTask());
            _lastTask = contTask;
            ((App) Application.Current).AddSessionTask(contTask);
            return contTask;
        }

        /// <summary>Deserialises a table from Json, throws excption on failure.</summary>
        /// <typeparam name="TPoco">The type od the table.</typeparam>
        /// <param name="tableName">The name of the table (used for errors).</param>
        /// <param name="response">The json to be deserialized.</param>
        /// <returns>Table entry objects.</returns>
        private static async Task<List<TPoco>> DeserializeTableThrowOnErrrorAsync<TPoco>(string tableName,
            string response) where TPoco : class
        {
            List<TPoco> updatesFromServer;
            try
            {
                updatesFromServer =
                    await Task.Run(() => JsonConvert.DeserializeObject<List<TPoco>>(response)).ConfigureAwait(false);
            }
            catch (JsonSerializationException e)
            {
                Debug.WriteLine($"Desserialise falied on response for {tableName}");
                Debug.WriteLine(e);
                throw new Exception($"Derserialize failed: {tableName}");
            }
            return updatesFromServer;
        }

        /// <summary>Gets a large tree of all of the data the UI uses except for live and historical readings.
        /// Run on threadpool because SQLite doesn't do async IO.</summary>
        /// <returns>Non-readings tree of data used bvy the UI.</returns>
        private List<CropCycle> GetDataTree()
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

        /// <summary>Downloads derserialzes and add/merges a table.</summary>
        /// <typeparam name="TPoco">The POCO type of the table.</typeparam>
        /// <param name="tableName">Name of the table to request.</param>
        /// <param name="dbTable">The actual POCO table.</param>
        /// <param name="cred">Credentials to be used to authenticate with the server. Only required for some
        /// types.</param>
        /// <returns>Null on success, otherwise an error message.</returns>
        private async Task<string> GetReqDeserMergeTable<TPoco>(string tableName, DbSet<TPoco> dbTable,
            Creds cred = null) where TPoco : class
        {
            // Any exeption raised in these methods result in abort exeption with an error message for debug.
            try
            {
                // Step 1: Request
                var response = await GetRequestTableThrowOnErrorAsync(tableName, cred);

                // Step 2: Deserialise
                var updatesFromServer = await DeserializeTableThrowOnErrrorAsync<TPoco>(tableName, response);
                Debug.WriteLineIf(updatesFromServer.Count > 0, $"Deserialised {updatesFromServer.Count} for {tableName}");

                // Step 3: Merge
                // Get the DbSet that this request should be inserted into.
                AddOrModify(updatesFromServer, dbTable);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return null;
        }

        /// <summary>Updates sensor history from server.</summary>
        /// <returns>Errors, null on succes.</returns>
        private static async Task<string> GetRequestSensorHistoryAsync()
        {
            var cred = Creds.FromUserIdAndToken(Settings.Instance.CredUserId, Settings.Instance.CredToken);

            using (var db = new MainDbContext())
            {
                var sensors = db.Sensors.AsNoTracking().ToList();

                // Each sensor has a history object for each day.
                foreach (var sensor in sensors)
                {
                    bool anythingDownloaded;
                    var lastUploadedTimestamp =
                        db.SensorsHistory.Where(sh => sh.SensorID == sensor.ID).Max(hist => hist.UploadedAt);
                    var unixTime = lastUploadedTimestamp == default(DateTimeOffset)
                        ? 0
                        : lastUploadedTimestamp.ToUnixTimeSeconds();

                    do
                    {
                        var tableName = nameof(db.SensorsHistory);

                        List<SensorHistory> daysDownloaded;
                        try
                        {
                            var download =
                                await
                                    GetRequestTableThrowOnErrorAsync(
                                            $"{tableName}/{sensor.ID}/{unixTime}/{MaximumDaysToDownload}", cred)
                                        .ConfigureAwait(false);

                            daysDownloaded =
                                await
                                    DeserializeTableThrowOnErrrorAsync<SensorHistory>(tableName, download)
                                        .ConfigureAwait(false);

                            anythingDownloaded = daysDownloaded.Any();
                        }
                        catch (Exception ex)
                        {
                            return ex.ToString();
                        }

                        Debug.WriteLine(
                            $"{daysDownloaded.Count} days downloaded for {sensor.SensorTypeID} " +
                            $"on {lastUploadedTimestamp.Date}");

                        foreach (var downloadedHistoryDay in daysDownloaded)
                        {
                            if (downloadedHistoryDay.UploadedAt > lastUploadedTimestamp)
                                lastUploadedTimestamp = downloadedHistoryDay.UploadedAt;

                            var existingHistoryDay =
                                db.SensorsHistory.FirstOrDefault(
                                    sh =>
                                        sh.TimeStamp.Date == downloadedHistoryDay.TimeStamp.Date &&
                                        sh.SensorID == sensor.ID);
                            try
                            {
                                if (existingHistoryDay == null)
                                {
                                    db.SensorsHistory.Add(downloadedHistoryDay);
                                }
                                else
                                {
                                    var merged = SensorHistory.Merge(existingHistoryDay, downloadedHistoryDay);
                                    db.SensorsHistory.Update(merged);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Failed to database.");
                            }
                        }


                    } while (anythingDownloaded);
                }

                db.SaveChanges();
                return null;
            }
        }

        /// <summary>Updates sensor history from server. Queued to run after existing database and server
        /// operations.</summary>
        /// <returns>Errors, null on succes.</returns>
        private async Task<string> GetRequestSensorHistoryAsyncQueued()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(GetRequestSensorHistoryAsync));
            var updatesFromServerAsync = await await cont.ConfigureAwait(false);
            await Messenger.Instance.TablesChanged.Invoke(null);
            return updatesFromServerAsync;
        }

        private static async Task<string> GetRequestTableThrowOnErrorAsync(string tableName, Creds cred)
        {
            string response;
            if (cred == null)
                response = await Request.GetTable(ApiUrl, tableName).ConfigureAwait(false);
            else
                response = await Request.GetTable(ApiUrl, tableName, cred).ConfigureAwait(false);

            if (response.StartsWith("Error:"))
            {
                Debug.WriteLine($"Request failed: {tableName}, creds {null == cred}, {response}");

                throw new Exception($"Request failed: {tableName}, {response}");
            }
            return response;
        }

        private async Task<List<string>> GetRequestUpdateAsync()
        {
            var settings = Settings.Instance;
            var creds = Creds.FromUserIdAndToken(settings.CredUserId, settings.CredToken);
            var now = DateTimeOffset.Now;

            var res = new List<string>();

            using (var db = new MainDbContext())
            {
                // Setting configure await to false allows all of this method to be run on the threadpool.
                // Without setting it false the continuation would be posted onto the SynchronisationContext, which is the UI.
                res.Add(await GetReqDeserMergeTable(nameof(db.Parameters), db.Parameters).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Placements), db.Placements).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Subsystems), db.Subsystems).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.RelayTypes), db.RelayTypes).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.SensorTypes), db.SensorTypes).ConfigureAwait(false));

                if (res.Any(r => r != null))
                {
                    return res.Where(r => r != null).ToList();
                }
                db.SaveChanges();

                // Editable types that must be merged:

                res.Add(await GetReqDeserMergeTable(nameof(db.People), db.People, creds).ConfigureAwait(false));
                // Crop type is the only mergable that is no-auth.
                res.Add(await GetReqDeserMergeTable(nameof(db.CropTypes), db.CropTypes).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Locations), db.Locations, creds).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.CropCycles), db.CropCycles, creds).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Devices), db.Devices, creds).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Relays), db.Relays, creds).ConfigureAwait(false));
                res.Add(await GetReqDeserMergeTable(nameof(db.Sensors), db.Sensors, creds).ConfigureAwait(false));

                if (res.Any(r => r != null))
                {
                    return res.Where(r => r != null).ToList();
                }
                db.SaveChanges();
            }

            res = res.Where(r => r != null).ToList();

            if (!res.Any())
            {
                settings.LastSuccessfulGeneralDbGet = now;
            }

            return res;
        }

        /// <summary>Updates the database from the cloud server.</summary>
        /// <returns>A compilation of errors.</returns>
        private async Task<List<string>> GetRequestUpdateAsyncQueued()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(GetRequestUpdateAsync));
            var updatesFromServerAsync = await await cont.ConfigureAwait(false);
            await Messenger.Instance.TablesChanged.Invoke(null);
            return updatesFromServerAsync;
        }

        private async Task<string> PostRequestHistoryAsyncQueued()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(PostRequestHistoryChangesAsync));
            return await await cont.ConfigureAwait(false);
        }

        /// <summary>Posts all new history items since the last time data was posted.</summary>
        /// <returns></returns>
        private static async Task<string> PostRequestHistoryChangesAsync()
        {
            var creds = Creds.FromUserIdAndToken(Settings.Instance.CredUserId, Settings.Instance.CredToken);
            using (var db = new MainDbContext())
            {
                var tableName = nameof(db.SensorsHistory);
                if (!db.SensorsHistory.Any()) return null;

                // This is a list of historical uploads 
                var needsPost =
                    new Queue<SensorHistory>(
                        db.SensorsHistory.Where(s => s.UploadedAt == default(DateTimeOffset)).AsNoTracking().ToList());


                while (needsPost.Count > 0)
                {
                    var batch = new List<SensorHistory>();
                    while (batch.Count < 30 && needsPost.Any())
                    {
                        var item = needsPost.Dequeue();
                        item.SerialiseData();
                        batch.Add(item);
                    }
                    var json = JsonConvert.SerializeObject(batch);
                    var result = await Request.PostTable(ApiUrl, tableName, json, creds);
                    if (result != null)
                    {
                        //abort
                        return result;
                    }
                }

                var posted = needsPost.Select(sh => Tuple.Create(sh.SensorID, sh.TimeStamp));

                var lastPostTime = Settings.Instance.LastHistoryPostTime;

                var recentlyChanged = db.SensorsHistory.Where(sh => sh.TimeStamp > lastPostTime).AsNoTracking().ToList();
                var locallyChanged = recentlyChanged.Where(sh =>
                {
                    var shKey = Tuple.Create(sh.SensorID, sh.TimeStamp);
                    return posted.Any(po => shKey.Equals(po));
                }).ToList();

                var localJson = JsonConvert.SerializeObject(locallyChanged);
                var localResult = await Request.PostTable(ApiUrl, tableName, localJson, creds);
                if (localResult != null)
                {
                    //abort
                    return localResult;
                }
            }
            return null;
        }

        /// <summary>Only supports tables that derive from BaseEntity and Croptype.</summary>
        /// <param name="table">The DBSet object for the table.</param>
        /// <param name="tableName">The name of the table in the API .</param>
        /// <param name="lastPost">The last time the table was synced.</param>
        /// <param name="creds">Authentication credentials.</param>
        /// <returns>Null on success otherwise an error message.</returns>
        private static async Task<string> PostRequestTableWhereUpdatedAsync(IQueryable<BaseEntity> table,
            string tableName, DateTimeOffset lastPost, Creds creds)
        {
            var edited = table.Where(t => t.UpdatedAt > lastPost).ToList();

            if (!edited.Any()) return null;

            var data = JsonConvert.SerializeObject(edited, Formatting.None);
            var req = await Request.PostTable(ApiUrl, tableName, data, creds).ConfigureAwait(false);
            return req;
        }

        /// <summary>Posts changes saved in the local DB (excluding histories) to the server. Only Items with
        /// UpdatedAt or CreatedAt changed since the last post are posted.</summary>
        private static async Task<List<string>> PostRequestUpdateAsync()
        {
            var creds = Creds.FromUserIdAndToken(Settings.Instance.CredUserId, Settings.Instance.CredToken);

            var lastDatabasePost = Settings.Instance.LastSuccessfulGeneralDbPost;
            var postTime = DateTimeOffset.Now;

            var responses = new List<string>();

            // Simple tables that change:
            // CropCycle, Devices.
            using (var db = new MainDbContext())
            {
                responses.Add(
                    await
                        PostRequestTableWhereUpdatedAsync(db.Locations, nameof(db.Locations), lastDatabasePost, creds)
                            .ConfigureAwait(false));

                // CropTypes is unique:
                var changedCropTypes = db.CropTypes.Where(c => c.CreatedAt > lastDatabasePost);

                if (changedCropTypes.Any())
                {
                    var cropTypeData = JsonConvert.SerializeObject(changedCropTypes);
                    responses.Add(
                        await Request.PostTable(ApiUrl, nameof(db.CropTypes), cropTypeData, creds).ConfigureAwait(false));
                }

                responses.Add(
                    await
                        PostRequestTableWhereUpdatedAsync(db.CropCycles, nameof(db.CropCycles), lastDatabasePost, creds)
                            .ConfigureAwait(false));
                responses.Add(
                    await
                        PostRequestTableWhereUpdatedAsync(db.Devices, nameof(db.Devices), lastDatabasePost, creds)
                            .ConfigureAwait(false));
                responses.Add(
                    await
                        PostRequestTableWhereUpdatedAsync(db.Sensors, nameof(db.Sensors), lastDatabasePost, creds)
                            .ConfigureAwait(false));
                responses.Add(
                    await
                        PostRequestTableWhereUpdatedAsync(db.Relays, nameof(db.Relays), lastDatabasePost, creds)
                            .ConfigureAwait(false));
            }

            var errors = responses.Where(r => r != null).ToList();
            if (!errors.Any()) Settings.Instance.LastSuccessfulGeneralDbPost = postTime;
            return errors;
        }

        private async Task<List<string>> PostRequestUpdatesAsyncQueued()
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(PostRequestUpdateAsync));
            return await await cont.ConfigureAwait(false);
        }
    }
}
