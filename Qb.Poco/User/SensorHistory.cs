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
        public Guid LocationId { get; set; }

        /// <summary>Raw data, numbers encoded to 64bit binary.</summary>
        /// <remarks>Don't JsonIgnore this, send it over the iterwebs as base64 (Json.Net deos this by default). Much more
        ///     efficient than mashing it down to json, and also a lot easier and consistent.</remarks>
        public byte[] RawData { get; set; } = new byte[0];


        /// <remarks>fk-nav</remarks>
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; }

        /// <remarks>fk</remarks>
        public Guid SensorId { get; set; }

        /// <summary>Use UTC time based date to set the beginning time. This avoids confusion over timezone changes.</summary>
        public DateTime UtcDate { get; set; }

        /// <summary>The datetime that this history was uploaded to the server, always set by the server and only by the server.
        ///     For the local computer this is a gaurantee that data up to this point has been downloaded from the server.</summary>
        public DateTimeOffset UploadedAt { get; set; } = default(DateTimeOffset);
    }
}