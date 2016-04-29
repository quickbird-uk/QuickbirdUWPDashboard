namespace DatabasePOCOs.User
{
    using System;

    public class BaseEntity : IHasGuid
    {
        /// <summary>
        ///     Don't change this after the object's creation
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        ///     Update the avlaue every time you edit the object
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        ///     used for soft-delete. As a rule of thumb, don't display these items in UI
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        ///     used to detect conflicts. To be implemented
        /// </summary>
        public byte[] Version { get; set; }

        public Guid ID { get; set; }
    }
}