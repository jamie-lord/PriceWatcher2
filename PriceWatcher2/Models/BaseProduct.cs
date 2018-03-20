namespace PriceWatcher2.Models
{
    public class BaseProduct : IBaseProduct
    {
        public string Id { get; set; }

        public string Rev { get; set; }

        public string ExternalId { get; set; }
    }
}
