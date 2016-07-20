namespace DbStructure
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using User;

    public class Device : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public Guid SerialNumber { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }
        [Required]
        public Guid LocationID { get; set; }

        [JsonIgnore]
        public virtual List<Sensor> Sensors { get; set; }

        [JsonIgnore]
        public virtual List<Relay> Relays { get; set; }
    }
}