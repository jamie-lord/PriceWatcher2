using System;

namespace PriceWatcher2.Models
{
    public class TescoProduct : BaseProduct
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string ImageUrl { get; set; }

        public DateTime Discovered { get; set; }

        public string InfoMessage { get; set; }
    }
}
