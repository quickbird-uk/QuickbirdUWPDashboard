namespace DatabasePOCOs.Global
{
    public class ControlType
    {
        public long ID { get; set; } 

        public string Name { get; set; }

        public bool Additive { get; set; }

        public Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }
    }
}
