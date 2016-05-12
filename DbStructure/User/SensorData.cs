namespace DatabasePOCOs.User
{
    using System;

    public class SensorData
    {
        public DateTimeOffset DateTime { get; set; }
        public virtual Sensor Sensor { get; set; }
        public Guid SensorID { get; set; }

        public virtual Site Site { get; set; }
        public Guid? SiteID { get; set; }

        public byte[] DayData { get; set; }
    }
}