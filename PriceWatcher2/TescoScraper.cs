using PriceWatcher2.Models;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PriceWatcher2
{
    public class TescoScraper
    {
        private ProductStore _productStore;

        public async Task Start()
        {
            await ProcessPage("https://www.tesco.com/groceries/en-GB/search?query=bakery");
        }

        public async Task ProcessPage(string uri)
        {
            _productStore = new ProductStore();

            ScrapingBrowser browser = new ScrapingBrowser();

            var startTimestamp = DateTime.Now;
            WebPage page = browser.NavigateToPage(new Uri(uri));

            var productTiles = page.Html.CssSelect("div.product-tile");

            foreach (var tile in productTiles)
            {
                var externalId = tile.CssSelect("div.tile-content").FirstOrDefault()?.Attributes.Where(x => x.Name == "id").FirstOrDefault()?.Value;

                if (string.IsNullOrWhiteSpace(externalId))
                {
                    Console.WriteLine("No external id found");
                    continue;
                }

                var existingProduct = await _productStore.FindProduct<TescoProduct>(externalId);
                var product = new TescoProduct();

                if (existingProduct != null)
                {
                    product = existingProduct;
                }
                else
                {
                    product.Discovered = startTimestamp;
                    product.ExternalId = externalId;
                }
                
                product.ImageUrl = tile.CssSelect("img.product-image").FirstOrDefault()?.Attributes.Where(x => x.Name == "data-src").FirstOrDefault()?.Value;
                product.Title = tile.CssSelect("a.product-tile--title").FirstOrDefault()?.InnerText;
                product.InfoMessage = tile.CssSelect("div.product-info-message-section > div.product-info-message").FirstOrDefault()?.InnerText;
                
                var url = tile.CssSelect("div.product-details--content > a").FirstOrDefault()?.Attributes.Where(x => x.Name == "href").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(url))
                {
                    product.Url = "https://www.tesco.com" + url;
                }

                var pr = await _productStore.StoreProduct(product);

                if (double.TryParse(tile.CssSelect("div.price-per-sellable-unit > div > span > span.value").FirstOrDefault()?.InnerText, out var p))
                {
                    var pd = new TescoPricePoint
                    {
                        ProductId = pr.Id,
                        Price = p,
                        Timestamp = startTimestamp
                    };
                    pd.Currency = tile.CssSelect("div.price-per-sellable-unit > div > span > span.currency").FirstOrDefault()?.InnerText;

                    if (double.TryParse(tile.CssSelect("div.price-per-quantity-weight > span > span.value").FirstOrDefault()?.InnerText, out var q))
                    {
                        pd.PricePerWeight = q;
                        pd.Weight = tile.CssSelect("div.price-per-quantity-weight > span.weight").FirstOrDefault()?.InnerText;
                    }

                    var r = await _productStore.StorePricePoint(pd);
                }
            }
        }
    }
}
