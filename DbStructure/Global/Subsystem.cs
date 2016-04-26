using System.Collections.Generic;

namespace DatabasePOCOs.Global
{
    public class Subsystem
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public virtual List<ControlType> ControlTypes { get; set;}
        
        public virtual List<ParamAtPlace> ParamsAtPlaces { get; set; }      
    }
}
