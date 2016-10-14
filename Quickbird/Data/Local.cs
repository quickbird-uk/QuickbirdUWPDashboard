namespace Quickbird.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Qb.Poco.Global;
    using Qb.Poco.User;

    internal static class Local
    {
        public static void AddAndUpdateHistories(List<SensorHistory> newHistories, List<SensorHistory> updatedHistories)
        {
            using (var db = new QbDbContext())
            {
                db.SensorsHistory.AddRange(newHistories);
                db.SensorsHistory.UpdateRange(updatedHistories);
                db.SaveChanges();
            }
        }

        public static void AddCropCycle(CropCycle cropCycle)
        {
            using (var db = new QbDbContext())
            {
                db.CropCycles.Add(cropCycle);
                db.SaveChanges();
            }
        }

        public static void AddDeviceWithItsLocationAndSensors(Device device)
        {
            using (var db = new QbDbContext())
            {
                db.Locations.Add(device.Location);
                db.Sensors.AddRange(device.Sensors);
                db.Devices.Add(device); // TODO: Find out if this works using only this line (nav properties exist).
                db.SaveChanges();
            }
        }

        public static CropCycle GetCropCycle(Guid cropCycleId)
        {
            using (var db = new QbDbContext())
            {
                var cropCycle = db.CropCycles.AsNoTracking().First(cc => cc.Id == cropCycleId);
                return cropCycle;
            }
        }

        public static List<CropCycle> GetCropCyclesThatEndedWithLocations()
        {
            using (var db = new QbDbContext())
            {
                var now = DateTimeOffset.Now;
                var cropCycles =
                    db.CropCycles.Where(cc => (cc.EndDate != null) && (cc.EndDate > now))
                        .Include(cc => cc.Location)
                        .AsNoTracking()
                        .ToList();
                return cropCycles;
            }
        }

        public static List<CropType> GetCropTypes()
        {
            using (var db = new QbDbContext())
            {
                var ct = db.CropTypes.AsNoTracking().ToList();
                return ct;
            }
        }

        public static async Task<List<CropCycle>> GetDataTree()
        {
            return await Task.Run(() =>
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
            });
        }

        public static List<Device> GetDevicesWithSensors()
        {
            using (var db = new QbDbContext())
            {
                var devices = db.Devices.Include(d => d.Sensors).AsNoTracking().ToList();
                return devices;
            }
        }

        public static List<Location> GetLocationsWithCropCyclesAndDevices()
        {
            using (var db = new QbDbContext())
            {
                var locations = db.Locations.Include(l => l.CropCycles).Include(l => l.Devices).AsNoTracking().ToList();
                return locations;
            }
        }

        /// <summary>May retrieve more data than asked for, does not splice.</summary>
        /// <param name="locationId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<SensorHistory> GetSensorHistoriesBetween(Guid locationId, DateTimeOffset startDate,
            DateTimeOffset endDate)
        {
            var utcStart = startDate.UtcDateTime.Date;
            var utcEnd = endDate.UtcDateTime.Date;
            using (var db = new QbDbContext())
            {
                var histories =
                    db.SensorsHistory.Where(
                            sh => (sh.UtcDate >= utcStart) && (sh.UtcDate <= utcEnd) && (sh.LocationId == locationId))
                        .AsNoTracking()
                        .ToList();
                return histories;
            }
        }

        public static List<Sensor> GetSensorsWithPlacementsParametersAndSubsystems()
        {
            using (var db = new QbDbContext())
            {
                var sensors =
                    db.Sensors.Include(s => s.SensorType.Parameter)
                        .Include(s => s.SensorType.Placement)
                        .Include(s => s.SensorType.Subsystem)
                        .AsNoTracking()
                        .ToList();
                return sensors;
            }
        }

        public static List<SensorType> GetSensorTypesWithParametersPlacementsAndSubsystems()
        {
            using (var db = new QbDbContext())
            {
                var sensorTypes =
                    db.SensorTypes.Include(st => st.Parameter)
                        .Include(st => st.Placement)
                        .Include(st => st.Subsystem)
                        .AsNoTracking()
                        .ToList();
                return sensorTypes;
            }
        }

        public static IEnumerable<SensorHistory> GetTodaysSensorHistories()
        {
            using (var db = new QbDbContext())
            {
                var utcNow = DateTime.UtcNow;
                var histories = db.SensorsHistory.Where(sh => sh.UtcDate < utcNow).AsNoTracking().ToList();
                return histories;
            }
        }

        public static void UpdateCurrentCropCycle(Guid cropCycleId, double yieldToAdd, bool closeCropRun)
        {
            using (var db = new QbDbContext())
            {
                var currentCropCycle = db.CropCycles.First(cc => cc.Id == cropCycleId);
                currentCropCycle.Yield += yieldToAdd;
                var now = DateTimeOffset.Now;
                if (closeCropRun) currentCropCycle.EndDate = now;
                currentCropCycle.UpdatedAt = now;
                db.SaveChanges();
            }
        }
    }
}
