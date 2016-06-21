namespace DatabasePOCOs
{
    using System;

    public interface IHasGuid
    {
        Guid ID { get; }
    }
}