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

        public virtual Greenhouse Greenhouse { get; set; }

        public Guid GreenhouseID { get; set; }

        public virtual List<Sensor> Sensors { get; set; }

        public virtual List<Relay> Relays { get; set; }
    }
}