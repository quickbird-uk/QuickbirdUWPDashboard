namespace DatabasePOCOs
{
    using System;
    using User;

    public class Relay : BaseEntity
    {
        public uint OnTime { get; set; }
        public uint OffTime { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public bool Enabled { get; set; } = false;

        public virtual Device Device { get; set; }
        public Guid DeviceID { get; set; }
    }
}