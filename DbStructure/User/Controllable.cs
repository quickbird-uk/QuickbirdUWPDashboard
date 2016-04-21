using DatabasePOCOs.Global;
using System;
using System.Collections.Generic;

namespace DatabasePOCOs.User
{
    public class Controllable : BaseEntity
    {
        public string Name { get; set; }

        public List<ControlHistory> ControlHistory {get; set;} 

        public Greenhouse Greenhouse { get; set; }

        public Guid GreenhouseID { get; set; }

        public ControlType ControlType { get; set; }

        public Guid ControlTypeID { get; set; }

        public Relay Relay { get; set; }
    }
}
