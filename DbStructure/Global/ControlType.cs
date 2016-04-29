namespace DatabasePOCOs.Global
{
    public class ControlType : IHasId
    {
        public string Name { get; set; }

        public bool Additive { get; set; }

        public virtual Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }
        public long ID { get; set; }
    }
}