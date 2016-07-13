namespace DbStructure.User
{
    using System;
    using System.ComponentModel.DataAnnotations;
    public class BaseEntity : IHasGuid
    {
        [Required]
        public Guid ID { get; set; }

        /// <summary>
        ///     Don't change this after the object's creation
        /// </summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        ///     Should be updated every time a value is locally changed.
        /// </summary>
        [Required]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        ///     used for soft-delete. As a rule of thumb, don't display these items in UI
        /// </summary>
        [Required]
        public bool Deleted { get; set; } = false;

        /// <summary>
        ///     used to detect conflicts. To be implemented
        /// </summary>
        public byte[] Version { get; set; }
    }
}