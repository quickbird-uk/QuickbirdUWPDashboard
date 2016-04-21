using System;

namespace DatabasePOCOs.User
{
    public class CropCycle: BaseEntity
    {
        public string Name { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public CropType CropType { get; set; }

        public Guid CropTypeID { get; set; }

        public Greenhouse Greenhouse { get; set; }

        public Guid GreenhouseID { get; set; }

    }
}
