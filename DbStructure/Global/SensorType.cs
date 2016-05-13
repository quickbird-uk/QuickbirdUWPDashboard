using Newtonsoft.Json;

namespace DatabasePOCOs.Global
{
    public class SensorType : IHasId
    {
        public long ID { get; set; }

        [JsonIgnore]
        public virtual Subsystem Subsystem { get; set; }

        public long SubsystemID { get; set; }

        [JsonIgnore]
        public virtual Placement Place { get; set; }

        public long PlaceID { get; set; }

        [JsonIgnore]
        public virtual Parameter Param { get; set; }

        public long ParamID { get; set; }
    }
}