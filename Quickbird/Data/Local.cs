namespace Quickbird.Data
{
    using System;
    using System.Collections.Generic;
    using Qb.Poco.User;

    internal class Local
    {
        public static List<Location> GetLocationsWithCropCyclesAndDevices() { throw new NotImplementedException(); }

        public static List<SensorHistory> GetSensorHistoriesBetween(Guid locationId, DateTimeOffset startDate,
            DateTimeOffset endDate)
        {
            throw new NotImplementedException();
        }

        public static List<Sensor> GetSensorsWithPlacementsParametersAndSubsystems()
        {
            throw new NotImplementedException();
        }
    }
}
