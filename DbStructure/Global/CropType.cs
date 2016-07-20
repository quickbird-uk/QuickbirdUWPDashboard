namespace DbStructure
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using User;

    public class CropType
    {
        [MaxLength(245)]
        public string Name { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public bool Approved { get; set; } = false;

        public Guid? CreatedBy { get; set; } = null;

        [JsonIgnore]
        public virtual List<CropCycle> CropCycles { get; set; }
    }
}