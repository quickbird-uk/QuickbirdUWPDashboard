
using System;
using System.Collections.Generic;

namespace DatabasePOCOs.User
{
    public class Greenhouse: BaseEntity
    {
        public string Name { get; set; }

        public List<CropCycle> CropCycles { get; set; } 

        public List<Controllable> Controllables { get; set; } 

        public virtual Person Person { get; set; }

        public Guid PersonId { get; set; }

        public virtual List<Device> Devices { get; set; }

        public virtual List<SensorData> SensorData { get; set; }
    }
}
