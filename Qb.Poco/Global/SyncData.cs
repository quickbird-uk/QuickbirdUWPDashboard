namespace Qb.Poco.Global
{
    public class SyncData
    {
        public SyncData(CropType[] cropTypes, Parameter[] parameters, Placement[] placements, Subsystem[] subSystems,
            SensorType[] sensorTypes)
        {
            CropTypes = cropTypes;
            Parameters = parameters;
            Placements = placements;
            SubSystems = subSystems;
            SensorTypes = sensorTypes;
        }

        public CropType[] CropTypes { get; }
        public Parameter[] Parameters { get; }
        public Placement[] Placements { get; }
        public Subsystem[] SubSystems { get; }
        public SensorType[] SensorTypes { get; }
    }
}