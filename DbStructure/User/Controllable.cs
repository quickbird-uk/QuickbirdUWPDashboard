namespace DatabasePOCOs.User
{
    using System;
    using System.Collections.Generic;
    using Global;

    public class Controllable : BaseEntity
    {
        public string Name { get; set; }

        public virtual List<ControlHistory> ControlHistory { get; set; }

        public virtual Site Site { get; set; }

        public Guid SiteID { get; set; }

        public virtual ControlType ControlType { get; set; }

        public long ControlTypeID { get; set; }

        public virtual Relay Relay { get; set; }

        public Guid? RelayID { get; set; }
        //ADD relay ID !@ 
    }
}