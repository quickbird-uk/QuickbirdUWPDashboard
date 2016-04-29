namespace DatabasePOCOs.Global
{
    using System.Collections.Generic;

    public class PlacementType : IHasId
    {
        public string Name { get; set; }

        public virtual List<Sensor> Sensors { get; set; }
        public long ID { get; set; }
    }
}