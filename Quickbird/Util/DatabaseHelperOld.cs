namespace Quickbird.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Qb.Poco.User;
    using Internet;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Newtonsoft.Json;
    using Qb.Poco;
    using Qb.Poco.Global;


    /// <summary>Methods to access and update the database. Methods that modify the database are queued to
    /// make sure they do not interfere with one another. </summary>
    [Obsolete]
    public class DatabaseHelper
    {
        /// <summary>The Url of the web api that is used to fetch data.</summary>
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";

        /// <summary>The maximum number of days to download at a time.</summary>
        private const int MaxDaysDl = 5;

        /// <summary>An complete task that can be have ContinueWith() called on it. Used to queue database
        /// tasks to make sure one completes before another starts.</summary>
        private Task _lastTask = Task.CompletedTask;

        private DatabaseHelper() { }

        /// <summary>Singleton instance accessor.</summary>
        public static DatabaseHelper Instance { get; } = new DatabaseHelper();

        /// <summary>Gets all of the non-reading data that the UI uses as a big tree starting from each crop
        /// cycle.</summary>
        /// <remarks>This does not modify data so it doen't neeed to be queued.</remarks>
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

        /// <summary>Gets the most recent sensor value and timestamp from the database. This method is not
        /// queued, but it does not edit so is fine.</summary>
        /// <param name="sensor">Sensor POCO object to get value for.</param>
        /// <returns>Null if fails to find, othewise the most recent value and its timestamp.</returns>
        public static Tuple<double, DateTimeOffset> QueryMostRecentSensorValue(Sensor sensor)
        {
            using (var db = new QbDbContext())
            {
                var newestHistoryForSensor =
                    db.SensorsHistory.AsNoTracking()
                        .Where(sh => sh.SensorId == sensor.Id)
                        .OrderByDescending(sh => sh.UtcDate)
                        .FirstOrDefault();

                if (null == newestHistoryForSensor) return null;

                var data = SensorDatapoint.Deserialise(newestHistoryForSensor.RawData);
                var newestDatapoint = data.OrderByDescending(d => d.Timestamp).FirstOrDefault();

                if (null == newestDatapoint) return null;

                return Tuple.Create(newestDatapoint.Value, newestDatapoint.Timestamp);
            }
        }

        /// <summary>Requires UI thread. Syncs databse data with the server.</summary>
        /// <remarks>This method runs on the UI thread, the queued methods need it, they hand off work to the
        /// threadpool as appropriate.</remarks>
        public async Task SyncWithServerAsync()
        {
            // Sharing a db context allows the use of caching within the context while still being safer than a leaky
            //  global context.
            using (var db = new QbDbContext())
            {
                var updateErrors = await GetRequestUpdateAsyncQueued(db);
                if (updateErrors?.Any() ?? false)
                {
                    Debug.WriteLine(updateErrors);
                    return;
                }

                var updateHistErrors = await GetRequestSensorHistoryAsyncQueued(db);
                if (updateHistErrors?.Any() ?? false)
                {
                    Debug.WriteLine(updateHistErrors);
                    return;
                }

                var postErrors = await PostRequestUpdatesAsyncQueued(db);
                if (postErrors?.Any() ?? false)
                {
                    Debug.WriteLine(string.Join(",", postErrors));
                    return;
                }

                var postHistErrors = await PostRequestHistoryAsyncQueued(db);
                if (postHistErrors?.Any() ?? false)
                {
                    Debug.WriteLine(postHistErrors);
                }
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
                        dbSet.AsNoTracking().Select(a => a).FirstOrDefault(d => ((IHasId) d).Id == ((IHasId) remote).Id);
                }
                else if (pocoType.GetInterfaces().Contains(typeof(IHasGuid)))
                {
                    local =
                        dbSet.AsNoTracking()
                            .Select(a => a)
                            .FirstOrDefault(d => ((IHasGuid) d).Id == ((IHasGuid) remote).Id);
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
            using (var db = new QbDbContext())
            {
                return
                    db.CropCycles.AsNoTracking()
                        .Include(cc => cc.Location)
                        .Include(cc => cc.CropType)
                        .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Parameter)
                        .Include(cc => cc.Location)
                        .ThenInclude(l => l.Devices)
                        .ThenInclude(d => d.Sensors)
                        .ThenInclude(s => s.SensorType)
                        .ThenInclude(st => st.Placement)
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
        /// <returns>Errors, null on succes. Some data may have been successfully saved alongside errors.</returns>
        private static async Task<string> GetRequestSensorHistoryAsync(QbDbContext db)
        {
            throw new NotImplementedException();
        }

        /// <summary>Updates sensor history from server. Queued to run after existing database and server
        /// operations.</summary>
        /// <param name="db"></param>
        /// <returns>Errors, null on succes.</returns>
        private async Task<string> GetRequestSensorHistoryAsyncQueued(QbDbContext db)
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(() => GetRequestSensorHistoryAsync(db)));
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

        private async Task<List<string>> GetRequestUpdateAsync(QbDbContext db)
        {
            var settings = Settings.Instance;
            Creds creds = null;
            var now = DateTimeOffset.Now;

            var res = new List<string>();


            // Setting configure await to false allows all of this method to be run on the threadpool.
            // Without setting it false the continuation would be posted onto the SynchronisationContext, which is the UI.
            res.Add(await GetReqDeserMergeTable(nameof(db.Parameters), db.Parameters).ConfigureAwait(false));
            res.Add(await GetReqDeserMergeTable(nameof(db.Placements), db.Placements).ConfigureAwait(false));
            res.Add(await GetReqDeserMergeTable(nameof(db.Subsystems), db.Subsystems).ConfigureAwait(false));
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
            res.Add(await GetReqDeserMergeTable(nameof(db.Sensors), db.Sensors, creds).ConfigureAwait(false));

            if (res.Any(r => r != null))
            {
                return res.Where(r => r != null).ToList();
            }
            db.SaveChanges();


            res = res.Where(r => r != null).ToList();

            if (!res.Any())
            {
                settings.LastSuccessfulGeneralDbGet = now;
            }

            return res;
        }

        /// <summary>Updates the database from the cloud server.</summary>
        /// <returns>A compilation of errors.</returns>
        private async Task<List<string>> GetRequestUpdateAsyncQueued(QbDbContext db)
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(() => GetRequestUpdateAsync(db)));
            var updatesFromServerAsync = await await cont.ConfigureAwait(false);
            await Messenger.Instance.TablesChanged.Invoke(null);
            return updatesFromServerAsync;
        }

        /// <summary>Posts all new history items since the last time data was posted.</summary>
        /// <returns></returns>
        private static async Task<string> PostRequestHistoryAsync(QbDbContext db)
        {
            throw new NotImplementedException();
        }

        private async Task<string> PostRequestHistoryAsyncQueued(QbDbContext db)
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(() => PostRequestHistoryAsync(db)));
            return await await cont.ConfigureAwait(false);
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
            var edited = table.AsNoTracking().Where(t => t.UpdatedAt > lastPost).ToList();

            if (!edited.Any()) return null;

            var data = JsonConvert.SerializeObject(edited, Formatting.None);
            var req = await Request.PostTable(ApiUrl, tableName, data, creds).ConfigureAwait(false);
            return req;
        }

        /// <summary>Posts changes saved in the local DB (excluding histories) to the server. Only Items with
        /// UpdatedAt or CreatedAt changed since the last post are posted.</summary>
        private static async Task<List<string>> PostRequestUpdateAsync(QbDbContext db)
        {
            Creds creds = null;

            var lastDatabasePost = Settings.Instance.LastSuccessfulGeneralDbPost;
            var postTime = DateTimeOffset.Now;

            var responses = new List<string>();

            // Simple tables that change:
            // CropCycle, Devices.

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


            var errors = responses.Where(r => r != null).ToList();
            if (!errors.Any()) Settings.Instance.LastSuccessfulGeneralDbPost = postTime;
            return errors;
        }

        private async Task<List<string>> PostRequestUpdatesAsyncQueued(QbDbContext db)
        {
            var cont = AttachContinuationsAndSwapLastTask(() => Task.Run(() => PostRequestUpdateAsync(db)));
            return await await cont.ConfigureAwait(false);
        }
    }
}
