namespace DatabasePOCOs
{
    using System;
    using System.Collections.Generic;
    using Global;
    using User;

    public class Sensor : BaseEntity
    {
        //Calibration Info
        public double Multiplier { get; set; } = 1;
        public double Offset { get; set; } = 0;

        public double? AlertHigh { get; set; } = null;
        public double? AlertLow { get; set; } = null;

        //This field should never be edited! 
        public virtual ParamAtPlace ParamAtPlace { get; set; }

        public long ParamAtPLaceID { get; set; }

        public virtual List<SensorData> SensorData { get; set; }

        public virtual Device Device { get; set; }
        public Guid DeviceID { get; set; }
    }
}