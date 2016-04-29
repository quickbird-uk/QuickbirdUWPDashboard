namespace DatabasePOCOs.User
{
    using System;

    public class ControlHistory
    {
        public Guid Controllable { get; set; }
        public Guid ControllableID { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public byte[] DataDay { get; set; }
        //    item.TimeStamp.T
        //    ControlDataItem item;
        //{ 

        //public List<ControlDataItem> OpenDataDay()
        //    int length = sizeof(bool) + sizeof(long) + sizeof(long); 
        //}
    }

    public struct ControlDataItem
    {
        public bool state;
        public DateTime TimeStamp;
        public TimeSpan Duration;
    }
}