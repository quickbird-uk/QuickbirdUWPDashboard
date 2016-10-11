using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Qb.Poco.User;

namespace Qb.Poco.Global
{
    public class CropType
    {
        /// <remarks>pk</remarks>
        public string Name { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public bool Deleted { get; set; } = false;

        /// <summary>The Person that created this croptype.</summary>
        public Guid? CreatedBy { get; set; }

        /// <remarks>nav</remarks>
        [JsonIgnore]
        public virtual List<CropCycle> CropCycles { get; set; }
    }
}