using Agronomist.Models;
using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 

namespace Agronomist.Util
{
    public class DatabaseHelper
    {
        private static DatabaseHelper _instance = null; 

        public static DatabaseHelper Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new DatabaseHelper(); 
                }
                return _instance; 
            }
        }


        Task _localTask;
        MainDbContext _db; 

        private DatabaseHelper()
        {
            var factory = new TaskFactory(TaskCreationOptions.LongRunning,
                                        TaskContinuationOptions.LongRunning);
            _localTask = factory.StartNew(() =>
            {
                 _db = new MainDbContext();  //bogus task just to create a task 
            }, TaskCreationOptions.LongRunning);
        }


        private List<CropCycle> _GetDatatree()
        {
            System.Diagnostics.Stopwatch sWatch = new System.Diagnostics.Stopwatch();
            sWatch.Start();

            var userData =  _db.CropCycles
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
                    .AsNoTracking().ToList();


            sWatch.Stop();
            System.Diagnostics.Debug.WriteLine($"retrieveing userData took {sWatch.ElapsedMilliseconds} milliseconds"); 
            return userData; 
        }

        public async Task<List<CropCycle>> GetDatatreeAsync()
        {
            return await _localTask.ContinueWith((Task previous) =>
            {
                var result = _GetDatatree();
                return result;
            }); 
        }

        public List<CropCycle> GetDatatree()
        {
            return _localTask.ContinueWith((Task previous) =>
            {
                var result = _GetDatatree();
                return result;
            }).Result; 
        }

        private List<KeyValuePair<Guid, List<SensorHistory>>> _GetDataReadings(List<Guid> SensorIDs, DateTimeOffset start, DateTimeOffset end)
        {
            DateTimeOffset trueStartDate = start.AddDays(-1); //Because of the way we put timestamps on days, we need to subtracks one

            List<SensorHistory> readings = _db.SensorsHistory.Where(sh => sh.TimeStamp > trueStartDate && sh.TimeStamp < end && SensorIDs.Contains(sh.SensorID))
                .AsNoTracking().ToList();

            List<KeyValuePair<Guid, List<SensorHistory>>> result = new List<KeyValuePair<Guid, List<SensorHistory>>>(); 

            foreach (SensorHistory sh in readings)
            {
                KeyValuePair<Guid, List<SensorHistory>> sensorCollection = result.FirstOrDefault(kv => kv.Key == sh.SensorID);
                //Check if this collection already exists
                if ((sensorCollection.Key == sh.SensorID) == false)
                {
                    sensorCollection = new KeyValuePair<Guid, List<SensorHistory>>(sh.SensorID, new List<SensorHistory>());
                    result.Add(sensorCollection); 
                }
                sensorCollection.Value.Add(sh); 
            }

            return result; 
        }

        public async Task<List<KeyValuePair<Guid, List<SensorHistory>>>> GetDataReadingAsync(List<Guid> SensorIDs, DateTimeOffset start, DateTimeOffset end)
        {
            return await _localTask.ContinueWith((Task previous) =>
            {
                var result = _GetDataReadings(SensorIDs, start, end);
                return result;
            });
        }

        public List<KeyValuePair<Guid, List<SensorHistory>>> GetDataReading(List<Guid> SensorIDs, DateTimeOffset start, DateTimeOffset end)
        {
            return _localTask.ContinueWith((Task previous) =>
            {
                var result = _GetDataReadings(SensorIDs, start, end);
                return result;
            }).Result;
        }
    }
}
