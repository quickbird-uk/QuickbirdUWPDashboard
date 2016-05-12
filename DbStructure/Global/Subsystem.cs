namespace DatabasePOCOs.Global
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Subsystem : IHasId
    {
        public long ID { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public virtual List<RelayType> ControlTypes { get; set; }

        [JsonIgnore]
        public virtual List<SensorType> SensorTypes { get; set; }
    }
}