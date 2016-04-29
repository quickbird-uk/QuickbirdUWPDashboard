namespace DatabasePOCOs.User
{
    using System;

    public class ControlHistory
    {
        public Guid Controllable { get; set; }
        public Guid ControllableID { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public byte[] DataDay { get; set; }

        //public List<ControlDataItem> OpenDataDay()
        //{ 
        //    ControlDataItem item;
        //    item.TimeStamp.T
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