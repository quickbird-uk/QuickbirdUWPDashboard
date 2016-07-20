namespace DbStructure
{
    using Global;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using User;

    public class Relay : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int OnTime { get; set; }
        [Required]
        public int OffTime { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public bool Enabled { get; set; } = false;

        [JsonIgnore]
        public virtual RelayType RelayType { get; set; }

        [Required]
        public long RelayTypeID { get; set; }

        [JsonIgnore]
        public virtual Device Device { get; set; }

        [Required]
        public Guid DeviceID { get; set; }


        [JsonIgnore]
        public virtual List<RelayHistory> RelayHistory { get; set; }
    }
}