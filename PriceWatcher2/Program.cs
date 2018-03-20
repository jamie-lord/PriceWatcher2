using System;

namespace PriceWatcher2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var t = new TescoScraper();
            t.Start().Wait();
        }
    }
}
