using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using SelfCheckoutServer.Tests.Factories;  
using HonourSelfCheckoutServer;      

namespace SelfCheckoutServer.Tests
{
    public class ProductsIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductsIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProductByBarcode_WithValidBarcode_ReturnsProduct()
        {

            string validBarcode = "1111111111111";
            int storeId = 1;


             var response = await _client.GetAsync($"/api/Products/GetProductByBarcode/{validBarcode}?storeId={storeId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


            var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(product);
            Assert.Equal(validBarcode, product.BarcodeId);
            Assert.True(product.ProductId > 0);
        }

        [Fact]
        public async Task GetProductByBarcode_WithInvalidBarcode_ReturnsNotFound()
        {

            string invalidBarcode = "0000000000000";


            var response = await _client.GetAsync($"/api/Products/GetProductByBarcode/{invalidBarcode}");


            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }


    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeId { get; set; }
        public double Price { get; set; }
    }
}
