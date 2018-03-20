using MyCouch;
using PriceWatcher2.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PriceWatcher2
{
    public class ProductStore
    {
        private const string USERNAME = "PriceWatcher";
        private const string PASSWORD = "PriceWatcher";
        private const string SERVER = "http://" + USERNAME + ":" + PASSWORD + "@localhost:5984";
        private const string DATABASE = "price_watcher";

        private static MyCouchStore _productStore;

        public ProductStore()
        {
            using (var client = new MyCouchServerClient(SERVER))
            {
                var d = client.Databases.PutAsync(DATABASE).Result;

                if (!client.Databases.HeadAsync(DATABASE).Result.IsSuccess)
                {
                    throw new Exception($"Database '{DATABASE}' not found!");
                }
            }

            using (var client = new MyCouchClient(SERVER, DATABASE))
            {
                if (!client.Documents.HeadAsync("_design/products").Result.IsSuccess)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var docName = "PriceWatcher2.DesignDocuments.Products.json";
                    using (Stream stream = assembly.GetManifestResourceStream(docName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string docString = reader.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(docString))
                        {
                            throw new Exception("Failed to load document json file");
                        }

                        var doc = client.Documents.PostAsync(docString).Result;

                        if (!doc.IsSuccess)
                        {
                            throw new Exception("Failed to setup default view document");
                        }
                    }
                }
            }

            _productStore = new MyCouchStore(SERVER, DATABASE);
        }

        public async Task<BaseProduct> StoreProduct(BaseProduct obj)
        {
            if (obj == null)
            {
                return null;
            }
            return await _productStore.StoreAsync(obj);
        }

        public async Task<BasePricePoint> StorePricePoint(BasePricePoint obj)
        {
            if (obj == null)
            {
                return null;
            }
            return await _productStore.StoreAsync(obj);
        }

        public async Task<T> FindProduct<T>(string externalId)
        {
            var query = new Query("products", "product_with_id")
            {
                Key = externalId
            };

            var result = await _productStore.QueryAsync<T>(query);

            if (result == null || !result.Any())
            {
                return default(T);
            }

            if (result.Count() != 1)
            {
                throw new Exception($"More than 1 product with external id: {externalId}");
            }
            return result.First().Value;
        }

    }
}
