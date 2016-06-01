namespace DatabasePOCOs
{
    using System;
    using System.Collections.Generic;
    using Global;
    using User;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    public class Sensor : BaseEntity
    {
        //Calibration Info
        [Required]
        public double Multiplier { get; set; } = 1;
        [Required]
        public double Offset { get; set; } = 0;

        public double? AlertHigh { get; set; } = null;

        public double? AlertLow { get; set; } = null;

        [Required]
        public bool Enabled { get; set; } = false; 

        //This field should never be edited! 
        [JsonIgnore]
        public virtual SensorType SensorType { get; set; }
        [Required]
        public long SensorTypeID { get; set; }

        [JsonIgnore]
        public virtual Device Device { get; set; }
        [Required]
        public Guid DeviceID { get; set; }

        [JsonIgnore]
        public virtual List<SensorHistory> SensorHistory { get; set; }
    }
}
