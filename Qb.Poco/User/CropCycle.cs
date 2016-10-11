using System;
using Newtonsoft.Json;
using Qb.Poco.Global;

namespace Qb.Poco.User
{
    public class CropCycle : BaseEntity
    {
        public string Name { get; set; }

        public double Yield { get; set; } = 0;

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        /// <remarks>fk</remarks>
        public string CropTypeName { get; set; }

        /// <remarks>fk</remarks>
        public Guid LocationId { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual CropType CropType { get; set; }

        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Location Location { get; set; }
    }
}