using System.Collections.Generic;
using Newtonsoft.Json;

namespace Qb.Poco.Global
{
    public class Placement : IHasId
    {
        public string Name { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<SensorType> SensorTypes { get; set; }

        public long Id { get; set; }
    }
}