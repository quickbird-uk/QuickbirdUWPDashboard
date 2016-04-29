namespace DatabasePOCOs.Global
{
    using System.Collections.Generic;

    public class Subsystem : IHasId
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public virtual List<ControlType> ControlTypes { get; set; }

        public virtual List<ParamAtPlace> ParamsAtPlaces { get; set; }
    }
}