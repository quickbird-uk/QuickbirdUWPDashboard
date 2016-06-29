namespace DbStructure.User
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class Location : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual Person Person { get; set; }

        [Required]
        public Guid PersonId { get; set; }

        [JsonIgnore]
        public virtual List<Device> Devices { get; set; }

        [JsonIgnore]
        public virtual List<CropCycle> CropCycles { get; set; }

        [JsonIgnore]
        public virtual List<SensorHistory> SensorHistory { get; set; }

        [JsonIgnore]
        public virtual List<RelayHistory> RelayHistory { get; set; }
    }

}