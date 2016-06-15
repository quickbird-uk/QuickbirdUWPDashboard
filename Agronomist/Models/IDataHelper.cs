using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agronomist.Models
{
    using DatabasePOCOs;
    using DatabasePOCOs.User;

    interface IDataHelper
    {
        Sensor GetSensor(Guid id);

        IEnumerable<CropCycle> GetCropCycles();
    }
}
