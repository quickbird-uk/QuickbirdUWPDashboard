namespace DatabasePOCOs.Global
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class Placement : IHasId
    {
        public long ID { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public virtual List<Sensor> Sensors { get; set; }
    }
}