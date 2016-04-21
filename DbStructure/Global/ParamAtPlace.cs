namespace DatabasePOCOs.Global
{
    public class ParamAtPlace
    {
        public long ID { get; set; }

        public Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }

        public PlacementType Place { get; set; }

        public long PlaceID { get; set; }

        public Parameter Param { get; set; }

        public long ParamID { get; set; }
    }
}
