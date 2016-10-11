using Newtonsoft.Json;

namespace Qb.Poco.Global
{
    public class SensorType : IHasId
    {
        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Subsystem Subsystem { get; set; }

        /// <remarks>fk</remarks>
        public long SubsystemId { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Placement Placement { get; set; }

        /// <remarks>fk</remarks>
        public long PlacementId { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Parameter Parameter { get; set; }

        /// <remarks>fk</remarks>
        public long ParameterId { get; set; }

        public long Id { get; set; }
    }
}