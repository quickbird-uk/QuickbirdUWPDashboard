using System;

namespace Qb.Poco.User
{
    public class BaseEntity : IHasGuid
    {
        /// <summary>Don't change this after the object's creation</summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>Should be updated every time a value is locally changed.</summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>For soft deleteion without breaking old relationships. Usually hidden from users.</summary>
        public bool Deleted { get; set; } = false;

        /// <summary>pk</summary>
        public Guid Id { get; set; }
    }
}