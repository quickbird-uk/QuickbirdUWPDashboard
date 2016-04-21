using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs
{
    public class Relay : BaseEntity
    {
        public uint OnTime { get; set; }
        public uint OffTime { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public bool Enabled { get; set; } = false; 

        public Controllable Controlable { get; set; }

        public Guid ControllableID { get; set; }

        public Device Device { get; set; }
        public Guid DeviceID { get; set; }
    }
}
