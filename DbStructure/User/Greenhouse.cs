
using System.Collections.Generic;

namespace DatabasePOCOs.User
{
    public class Greenhouse: BaseEntity
    {
        public string Name { get; set; }

        public List<CropCycle> CropCycles { get; set; } 

        public List<Controllable> Controllables { get; set; } 

        public Person Person { get; set; }

        public long PersonId { get; set; }

        public List<Device> Devices { get; set; }

        public List<SensorData> SensorData { get; set; }
    }
}
