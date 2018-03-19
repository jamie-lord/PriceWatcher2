namespace PriceWatcher2.Models
{
    public class TescoPricePoint : BasePricePoint
    {
        public string Currency { get; set; }

        public double PricePerWeight { get; set; }

        public string Weight { get; set; }
    }
}
