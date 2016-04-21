using System.Collections.Generic;

namespace DatabasePOCOs.Global
{
    public class Subsystem
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public List<ControlType> ControlTypes { get; set;}
        
        public List<ParamAtPlace> ParamsAtPlaces { get; set; }      
    }
}
