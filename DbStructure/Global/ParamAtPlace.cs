namespace DatabasePOCOs.Global
{
    public class ParamAtPlace : IHasId
    {
        public long ID { get; set; }

        public virtual Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }

        public virtual PlacementType Place { get; set; }

        public long PlaceID { get; set; }

        public virtual Parameter Param { get; set; }

        public long ParamID { get; set; }
    }
}