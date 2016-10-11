namespace Qb.Poco.User
{
    /// <summary>All the sync data types (expluding SensorHistory) together for serialization.</summary>
    /// <remarks>The lists may be empty if there are no changes.</remarks>
    /// <remarks>Not a db class.</remarks>
    public class SyncData
    {
        public SyncData(long sourceDateTime, Person[] people, Location[] locations, Device[] devices,
            CropCycle[] cropCycles, Sensor[] sensors)
        {
            SourceDateTime = sourceDateTime;
            People = people;
            Locations = locations;
            Devices = devices;
            CropCycles = cropCycles;
            Sensors = sensors;
        }

        /// <summary>The time just before the data was queried from the DB.</summary>
        public long SourceDateTime { get; }

        /// <summary>Will normally return a single entry or no entries if there have been no changes.</summary>
        public Person[] People { get; }

        public Location[] Locations { get; }
        public Device[] Devices { get; }
        public CropCycle[] CropCycles { get; }
        public Sensor[] Sensors { get; }
    }
}