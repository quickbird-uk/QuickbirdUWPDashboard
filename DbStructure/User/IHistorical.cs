namespace DatabasePOCOs.User
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IHistorical
    {
        object Thing { get; set; }
        Guid ThingID { get; set; }
        Location Location { get; set; }
        Guid? LocationID { get; set; }
        DateTimeOffset TimeStamp { get; set; }
        byte[] RawData { get; set; }
        IList ThingData { get; set; }
        void SerialiseData();
        void DeserialiseData();
    }
}