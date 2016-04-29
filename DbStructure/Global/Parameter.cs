namespace DatabasePOCOs.Global
{
    public class Parameter : IHasId
    {
        public string Name { get; set; }

        public string Unit { get; set; }
        public long ID { get; set; }
    }
}