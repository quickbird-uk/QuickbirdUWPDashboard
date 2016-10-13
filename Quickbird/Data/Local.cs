namespace Quickbird.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public static void AddDeviceWithItsLocationAndSensors(Device device)
        {
            using (var db = new QbDbContext())
            {
                db.Locations.Add(device.Location);
                db.Sensors.AddRange(device.Sensors);
                db.Devices.Add(device); // TODO: Find out if this works using only this line (nav properties exist).
            }
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
    }
}
