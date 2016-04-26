using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs
{
    public class Device : BaseEntity
    {
        public string Name { get; set; }

        public Guid SerialNumber { get; set; }

        public string Location { get; set; }

        public virtual Greenhouse Greenhouse { get; set; }

        public Guid GreenhouseID { get; set; }

        public virtual List<Sensor> Sensors { get; set; }

        public virtual List<Relay> Relays { get; set; }
    }
}
 