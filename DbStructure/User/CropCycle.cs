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

        public virtual Site Site { get; set; }

        [Required]
        public Guid SiteID { get; set; }
    }
}