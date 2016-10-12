using System;

namespace Qb.Poco.User
{
    public class BaseEntity : IHasGuid
    {
        /// <summary>Don't change this after the object's creation</summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>Automatically set by server when added or updated (SaveChanges should be overridden), locally set when
        ///     changed on the client side.</summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>For soft deleteion without breaking old relationships. Usually hidden from users.</summary>
        public bool Deleted { get; set; } = false;

        /// <summary>pk</summary>
        public Guid Id { get; set; }
    }
}