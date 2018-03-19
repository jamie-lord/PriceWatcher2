using HtmlAgilityPack;
using MyCouch;
using PriceWatcher2.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PriceWatcher2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Setup();

            ScrapingBrowser browser = new ScrapingBrowser();

            var startTimestamp = DateTime.Now;
            WebPage page = browser.NavigateToPage(new Uri("https://www.tesco.com/groceries/en-GB/search?query=bakery"));

            var productTiles = page.Html.CssSelect("div.product-tile");

            foreach (var tile in productTiles)
            {
                var product = new TescoProduct
                {
                    Discovered = startTimestamp,
                    ImageUrl = tile.CssSelect("img.product-image").FirstOrDefault()?.Attributes.Where(x => x.Name == "data-src").FirstOrDefault()?.Value,
                    Title = tile.CssSelect("a.product-tile--title").FirstOrDefault()?.InnerText,
                    InfoMessage = tile.CssSelect("div.product-info-message-section > div.product-info-message").FirstOrDefault()?.InnerText
                };

                product.TescoId = tile.CssSelect("div.tile-content").FirstOrDefault()?.Attributes.Where(x => x.Name == "id").FirstOrDefault()?.Value;
                var url = tile.CssSelect("div.product-details--content > a").FirstOrDefault()?.Attributes.Where(x => x.Name == "href").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    product.Url = "https://www.tesco.com" + url;
                }

                if (double.TryParse(tile.CssSelect("div.price-per-sellable-unit > div > span > span.value").FirstOrDefault()?.InnerText, out var p))
                {
                    var pd = new TescoPricePoint
                    {
                        Price = p,
                        Timestamp = startTimestamp
                    };
                    pd.Currency = tile.CssSelect("div.price-per-sellable-unit > div > span > span.currency").FirstOrDefault()?.InnerText;

                    if (double.TryParse(tile.CssSelect("div.price-per-quantity-weight > span > span.value").FirstOrDefault()?.InnerText, out var q))
                    {
                        pd.PricePerWeight = q;
                        pd.Weight = tile.CssSelect("div.price-per-quantity-weight > span.weight").FirstOrDefault()?.InnerText;
                    }

                    product.PriceData.Add(pd);
                }

                var t = _commentStore.StoreAsync(product).Result;
            }

            Console.ReadLine();
        }

        private const string USERNAME = "CommentService";
        private const string PASSWORD = "CommentService";
        private const string SERVER = "http://" + USERNAME + ":" + PASSWORD + "@localhost:5984";
        private const string DATABASE = "products";

        private static MyCouchStore _commentStore;

        public static void Setup()
        {
            using (var client = new MyCouchServerClient(SERVER))
            {
                var d = client.Databases.PutAsync(DATABASE).Result;

                if (!client.Databases.HeadAsync(DATABASE).Result.IsSuccess)
                {
                    throw new Exception($"Database '{DATABASE}' not found!");
                }
            }

            //using (var client = new MyCouchClient(SERVER, DATABASE))
            //{
            //    if (!client.Documents.HeadAsync("_design/comments").Result.IsSuccess)
            //    {
            //        var assembly = Assembly.GetExecutingAssembly();
            //        var docName = "SocketComment.DesignDocuments.Comments.json";
            //        using (Stream stream = assembly.GetManifestResourceStream(docName))
            //        using (StreamReader reader = new StreamReader(stream))
            //        {
            //            string docString = reader.ReadToEnd();

            //            if (string.IsNullOrWhiteSpace(docString))
            //            {
            //                throw new Exception("Failed to load document json file");
            //            }

            //            var doc = client.Documents.PostAsync(docString).Result;

            //            if (!doc.IsSuccess)
            //            {
            //                throw new Exception("Failed to setup default view document");
            //            }
            //        }
            //    }
            //}

            _commentStore = new MyCouchStore(SERVER, DATABASE);
        }
    }
}
