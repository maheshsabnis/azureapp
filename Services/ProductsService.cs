using MVC_CosmosDB.Models;

namespace MVC_CosmosDB.Services
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;

    public static class ProductsService
    {
        // the DatabaseID
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"] ?? "ProductsDb";
        // the container aka collection 
        private static readonly string ContainerId = ConfigurationManager.AppSettings["container"] ?? "Products";
        // the CosmosDB Account EndPoint URI
        private static readonly string Endpoint = ConfigurationManager.AppSettings["endpoint"];
        // the access primary key
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["primaryKey"];
        private static CosmosItems items;
        private static CosmosClient client;

        /// <summary>
        /// Read the Product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public static async Task<ProductInfo> GetProductInfoAsync(string id, string partitionKey)
        {
            ProductInfo item = await items.ReadItemAsync<ProductInfo>(partitionKey, id);
            return item;
        }

        public static async Task<IEnumerable<ProductInfo>> GetOpenProductsAsync()
        {
            var queryText = "SELECT* FROM c";
            var querySpec = new CosmosSqlQueryDefinition(queryText);

            // Select all products. 
            // The max concurrency is set to 10, 
            // which controls the max number of partitions so that 
            // clients can query in parallel.
            var query = items.CreateItemQuery<ProductInfo>(querySpec, maxConcurrency: 10);

            List<ProductInfo> results = new List<ProductInfo>();
            while (query.HasMoreResults)
            {
                var set = await query.FetchNextSetAsync();
                results.AddRange(set);
            }

            return results;
        }

        /// <summary>
        /// Create the Product Record
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<ProductInfo> CreateProductAsync(ProductInfo item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString();
            }
            // the CategoryName is the partition key
            return await items.CreateItemAsync<ProductInfo>(item.CategoryName, item);
        }

        /// <summary>
        /// Update the Product
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static async Task<ProductInfo> UpdateProductAsync(ProductInfo item)
        {
            return await items.ReplaceItemAsync<ProductInfo>(item.CategoryName, item.Id, item);
        }

        /// <summary>
        /// Delete the Product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="catName"></param>
        /// <returns></returns>
        public static async Task DeleteProductAsync(string id, string catName)
        {
            await items.DeleteItemAsync<ProductInfo>(catName, id);
        }

        /// <summary>
        /// Initialize the db 
        /// </summary>
        /// <returns></returns>
        public static async Task InitDbConnection()
        {
            CosmosConfiguration config = new CosmosConfiguration(Endpoint, PrimaryKey);
            client = new CosmosClient(config);
            CosmosDatabase database = await client.Databases.CreateDatabaseIfNotExistsAsync(DatabaseId);
            CosmosContainer container = await database.Containers.CreateContainerIfNotExistsAsync(ContainerId, "/CategoryName");
            items = container.Items;
        }
    }
}