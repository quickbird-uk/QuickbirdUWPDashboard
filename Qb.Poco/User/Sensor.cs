using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Qb.Poco.Global;

namespace Qb.Poco.User
{
    public class Sensor : BaseEntity
    {
        public bool Enabled { get; set; } = false;

        /// <remarks>fk-nav, constant.</remarks>
        [JsonIgnore]
        public virtual SensorType SensorType { get; set; }

        /// <remarks>fk</remarks>
        public long SensorTypeId { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Device Device { get; set; }

        /// <remarks>fk</remarks>
        public Guid DeviceId { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<SensorHistory> SensorHistories { get; set; }
    }
}