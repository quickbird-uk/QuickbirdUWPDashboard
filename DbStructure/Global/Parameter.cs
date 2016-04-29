namespace DatabasePOCOs.Global
{
    public class Parameter : IHasId
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }
    }
}