namespace DatabasePOCOs.User
{
    using System;
    using System.Collections.Generic;

    public class Person : IHasGuid
    {
        public ulong twitterID { get; set; }

        public string TwitterHandle { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        public List<Greenhouse> Greenhouses { get; set; }
        public Guid ID { get; set; }
    }
}