namespace DatabasePOCOs.Global
{
    public class ControlType : IHasId
    {
        public long ID { get; set; } 

        public string Name { get; set; }

        public bool Additive { get; set; }

        public virtual Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }
    }
}