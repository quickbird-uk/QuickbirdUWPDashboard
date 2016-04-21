using System.Collections.Generic;

namespace DatabasePOCOs.Global
{
    public class PlacementType
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public List<Sensor> Sensors { get; set; }
    }
}
