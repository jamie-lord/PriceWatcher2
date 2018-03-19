using System;
using System.Collections.Generic;

namespace PriceWatcher2.Models
{
    public class TescoProduct
    {
        public string Id { get; set; }

        public string Rev { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string ImageUrl { get; set; }

        private List<TescoPricePoint> _priceData;

        public List<TescoPricePoint> PriceData
        {
            get
            {
                if (_priceData == null)
                {
                    _priceData = new List<TescoPricePoint>();
                }
                return _priceData;
            }
            set { _priceData = value; }
        }

        public DateTime Discovered { get; set; }

        public string InfoMessage { get; set; }

        public string TescoId { get; set; }
    }
}
