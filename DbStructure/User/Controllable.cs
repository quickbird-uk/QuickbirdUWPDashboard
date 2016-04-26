using DatabasePOCOs.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabasePOCOs.User
{
    public class Controllable : BaseEntity
    {
        public string Name { get; set; }

        public virtual List<ControlHistory> ControlHistory { get; set; }

        public virtual Greenhouse Greenhouse { get; set; }

        public Guid GreenhouseID { get; set; }

        public virtual ControlType ControlType { get; set; }

        public long ControlTypeID { get; set; }

        public virtual Relay Relay { get; set; }

        public Guid? RelayID {get; set;} 
        //ADD relay ID !@ 
    }
}
