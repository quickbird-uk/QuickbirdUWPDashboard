using Newtonsoft.Json;

namespace DatabasePOCOs.Global
{
    public class RelayType : IHasId
    {
        public long ID { get; set; } 

        public string Name { get; set; }

        public bool Additive { get; set; }

        [JsonIgnore]
        public virtual Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }
    }
}