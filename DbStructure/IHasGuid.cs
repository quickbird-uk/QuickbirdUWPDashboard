namespace DatabasePOCOs
{
    using System;

    internal interface IHasGuid
    {
        Guid ID { get; }
    }
}