using System.Collections.Generic;
using Newtonsoft.Json;

namespace Qb.Poco.Global
{
    public class Parameter : IHasId
    {
        /// <summary>Descriptive name for measurement.</summary>
        public string Name { get; set; }

        /// <summary>The displayed unit for this measurement.</summary>
        public string Unit { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<SensorType> SensorTypes { get; set; }

        public long Id { get; set; }
    }
}