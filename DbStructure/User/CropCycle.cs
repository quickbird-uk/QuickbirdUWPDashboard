namespace DatabasePOCOs.User
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CropCycle : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public virtual CropType CropType { get; set; }

        [Required]
        public string CropTypeName { get; set; }

        public virtual Greenhouse Greenhouse { get; set; }

        [Required]
        public Guid GreenhouseID { get; set; }
    }
}