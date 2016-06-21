namespace Agronomist.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.EntityFrameworkCore;
    using Models;

    /// <summary>
    ///     The public methods in theis helper are meant to be run on the UI thread - this is required to force the methods to
    ///     be executed consecutively and prevent overlap.
    /// </summary>
    public class DatabaseHelper
    {
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

        private async void UpdateFromServer()
        {
            using (var db = new MainDbContext())
            {
                // No auth no post types:
                // 1. Parameters
                // 2. --------------------------People WRONG
                // 3. Placements
                // 4. RelayTypes
                // 5. SensorTypes
                // 6. Subsystems

                var responses = new List<string>();
                responses.Add(await DownloadDeserialiseTable<Parameter>(nameof(db.Parameters)));
                responses.Add(await DownloadDeserialiseTable<Placement>(nameof(db.Placements)));
                responses.Add(await DownloadDeserialiseTable<Subsystem>(nameof(db.Subsystems)));
                responses.Add(await DownloadDeserialiseTable<RelayType>(nameof(db.RelayTypes)));
                responses.Add(await DownloadDeserialiseTable<SensorType>(nameof(db.SensorTypes)));


                // Editable types that must be merged.
                // 1.CropCycles
                // 2.CropType (uniqley does not require auth on get).
                // 3.Devices
                // 4.Locations
                // 5.Relays
                // 6.

                responses.Add(await DownloadDeserialiseTable<Person>(nameof(db.People), creds));
                // Crop type is the only mergable that is no-auth.
                responses.Add(await DownloadDeserialiseTable<CropType>(nameof(db.CropTypes)));
                responses.Add(await DownloadDeserialiseTable<Location>(nameof(db.Locations), creds));
                responses.Add(await DownloadDeserialiseTable<CropCycle>(nameof(db.CropCycles), creds));
                responses.Add(await DownloadDeserialiseTable<Device>(nameof(db.Devices), creds));
                responses.Add(await DownloadDeserialiseTable<Relay>(nameof(db.Relays), creds));
                responses.Add(await DownloadDeserialiseTable<Sensor>(nameof(db.Sensors), creds));


                // Items that have to get got in time slices.
                // 1.RelayHistory
                // 2.SensorHistory

                var unixtime = lastUpdate == default(DateTimeOffset) ? 0 : lastUpdate.ToUnixTimeSeconds();

                responses.Add(
                    await DownloadDeserialiseTable<SensorHistory>($"{nameof(db.SensorsHistory)}/{unixtime}/9001", creds));
                responses.Add(await DownloadDeserialiseTable<RelayHistory>($"{nameof(db.RelayHistory)}/{unixtime}/9001", creds));
                
            }

        }
    }
}