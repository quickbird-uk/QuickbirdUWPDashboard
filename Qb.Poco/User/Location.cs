using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Qb.Poco.User
{
    public class Location : BaseEntity
    {
        public string Name { get; set; }

        /// <remarks>fk</remarks>
        public Guid? PersonId { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Person Person { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<Device> Devices { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<CropCycle> CropCycles { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<SensorHistory> SensorHistories { get; set; }
    }
}