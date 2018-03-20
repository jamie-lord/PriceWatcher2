using System;

namespace PriceWatcher2.Models
{
    public class BasePricePoint : IBaseObject
    {
        public string Id { get; set; }

        public string Rev { get; set; }

        public string ProductId { get; set; }

        public double Price { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
