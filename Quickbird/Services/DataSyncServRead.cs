using DbStructure;
using DbStructure.User;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quickbird.Internet;
using Quickbird.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickbird.Services
{
    /// <summary>
    ///  Contains operations that do not affect the data itself. They are either stateless or read-only. 
    /// </summary>
    public partial class DataService
    {
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
                    db.CropCycles.AsNoTracking()
                        .Include(cc => cc.Location)
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
                        .ToList();
            }
        }

        /// <summary>Gets the most recent sensor value and timestamp from the database. This method is not
        /// queued, but it does not edit so is fine.</summary>
        /// <param name="sensor">Sensor POCO object to get value for.</param>
        /// <returns>Null if fails to find, othewise the most recent value and its timestamp.</returns>
        public static Tuple<double, DateTimeOffset> QueryMostRecentSensorValue(Sensor sensor)
        {
            using (var db = new MainDbContext())
            {
                var newestHistoryForSensor =
                    db.SensorsHistory.AsNoTracking()
                        .Where(sh => sh.SensorID == sensor.ID)
                        .OrderByDescending(sh => sh.TimeStamp)
                        .FirstOrDefault();

                if (null == newestHistoryForSensor) return null;

                newestHistoryForSensor.DeserialiseData();

                var newestDatapoint = newestHistoryForSensor.Data.OrderByDescending(d => d.TimeStamp).FirstOrDefault();

                if (null == newestDatapoint) return null;

                return Tuple.Create(newestDatapoint.Value, newestDatapoint.TimeStamp);
            }
        }

    }


}
