namespace DatabasePOCOs
{
    using System;
    using System.Collections.Generic;
    using User;

    public class Device : BaseEntity
    {
        public string Name { get; set; }

        public Guid SerialNumber { get; set; }

        public string Location { get; set; }

        public virtual Site Site { get; set; }

        public Guid SiteID { get; set; }

        public virtual List<Sensor> Sensors { get; set; }

        public virtual List<Relay> Relays { get; set; }
    }
}