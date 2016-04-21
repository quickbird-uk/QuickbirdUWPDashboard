using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs.User
{
    public class Person
    {
        public long ID { get; set; }

        public ulong twitterID { get; set; }

        public string TwitterHandle { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public List<Greenhouse> Greenhouses { get; set; }
    }
}
