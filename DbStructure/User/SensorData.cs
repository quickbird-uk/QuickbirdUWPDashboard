namespace DatabasePOCOs.User
{
    using System;

    public class SensorData
    {
        public DateTimeOffset DateTime { get; set; }
        public virtual Sensor Sensor { get; set; }
        public Guid SensorID { get; set; }

        public virtual Greenhouse Greenhouse { get; set; }
        public Guid? GreenhouseID { get; set; }

        public byte[] DayData { get; set; }
    }
}