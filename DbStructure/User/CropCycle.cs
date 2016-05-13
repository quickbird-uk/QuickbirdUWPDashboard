namespace DatabasePOCOs.User
{
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CropCycle : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        [JsonIgnore]
        public virtual CropType CropType { get; set; }

        [Required]
        public string CropTypeName { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }

        [Required]
        public Guid LocationID { get; set; }
    }
}