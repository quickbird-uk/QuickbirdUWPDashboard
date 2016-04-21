using System.Collections.Generic;

namespace DatabasePOCOs.User
{
    public class CropType : BaseEntity
    {
        public string Name { get; set; }

        public List<CropCycle> CropCycles { get; set; }
    }
}
