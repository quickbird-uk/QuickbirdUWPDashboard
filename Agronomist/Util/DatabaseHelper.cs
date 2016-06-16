namespace Agronomist.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using DatabasePOCOs.User;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class DatabaseHelper
    {
        private readonly Task _localTask;
        private MainDbContext _db;

        private DatabaseHelper()
        {
            var factory = new TaskFactory(TaskCreationOptions.LongRunning,
                TaskContinuationOptions.LongRunning);
            _localTask = factory.StartNew(() => {
                                                    _db = new MainDbContext(); //bogus task just to create a task 
            }, TaskCreationOptions.LongRunning);
        }

        public static DatabaseHelper Instance { get; } = new DatabaseHelper();


        private List<CropCycle> _GetDatatree()
        {
            var sWatch = new Stopwatch();
            sWatch.Start();

            var userData = _db.CropCycles
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
            Debug.WriteLine($"retrieveing userData took {sWatch.ElapsedMilliseconds} milliseconds");
            return userData;
        }

        public async Task<List<CropCycle>> GetDatatreeAsync()
        {
            return await _localTask.ContinueWith(previous =>
            {
                var result = _GetDatatree();
                return result;
            });
        }

        public List<CropCycle> GetDatatree()
        {
            return _localTask.ContinueWith(previous =>
            {
                var result = _GetDatatree();
                return result;
            }).Result;
        }

        private List<KeyValuePair<Guid, List<SensorHistory>>> _GetDataReadings(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            var trueStartDate = start.AddDays(-1);
                //Because of the way we put timestamps on days, we need to subtracks one

            var readings =
                _db.SensorsHistory.Where(
                    sh => sh.TimeStamp > trueStartDate && sh.TimeStamp < end && sensorIDs.Contains(sh.SensorID))
                    .AsNoTracking().ToList();

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

        public async Task<List<KeyValuePair<Guid, List<SensorHistory>>>> GetDataReadingAsync(List<Guid> sensorIDs,
            DateTimeOffset start, DateTimeOffset end)
        {
            return await _localTask.ContinueWith(previous =>
            {
                var result = _GetDataReadings(sensorIDs, start, end);
                return result;
            });
        }

        public List<KeyValuePair<Guid, List<SensorHistory>>> GetDataReading(List<Guid> sensorIDs, DateTimeOffset start,
            DateTimeOffset end)
        {
            return _localTask.ContinueWith(previous =>
            {
                var result = _GetDataReadings(sensorIDs, start, end);
                return result;
            }).Result;
        }
    }
}