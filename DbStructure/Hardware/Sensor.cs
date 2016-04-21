using DatabasePOCOs.Global;
using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs
{
    public class Sensor: BaseEntity
    {

        //Calibration Info
        public double Multiplier { get; set; } = 1;
        public double Offset { get; set; } = 0;

        public double? AlertHigh { get; set; } = null;
        public double? AlertLow { get; set; } = null; 

        //This field should never be edited! 
        public ParamAtPlace ParamAtPlace { get; set; }

        public long ParamAtPLaceID { get; set; }

        public List<SensorData> SensorData { get; set; }

        public Device Device { get; set; }
        public Guid DeviceID { get; set; }

    }
}
