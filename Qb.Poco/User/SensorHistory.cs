using System;
using Newtonsoft.Json;

namespace Qb.Poco.User
{
    public class SensorHistory
    {
        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Location Location { get; set; }

        /// <remarks>fk</remarks>
        public Guid? LocationId { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; } = new byte[0];


        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; }

        /// <remarks>fk</remarks>
        public Guid SensorId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>The datetime that this history was uploaded to the server, always set by the server and only by the server.
        ///     For the local computer this is a gaurantee that data up to this point has been downloaded from the server.</summary>
        public DateTimeOffset UploadedAt { get; set; } = default(DateTimeOffset);
    }
}