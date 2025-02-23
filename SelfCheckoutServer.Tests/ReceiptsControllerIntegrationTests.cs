using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using Microsoft.Extensions.DependencyInjection;
using SelfCheckoutServer.Tests.Factories;
using Xunit;

namespace SelfCheckoutServer.Tests
{
    public class ReceiptsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ReceiptsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUserReceipts_ReturnsReceiptsSortedByPurchaseDate()
        {
            // Arrange: Seed the database with a store, product, store product, and two receipts for UserId = 1.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                // Clear existing data.
                db.Receipts.RemoveRange(db.Receipts);
                db.ReceiptItems.RemoveRange(db.ReceiptItems);
                db.StoreProducts.RemoveRange(db.StoreProducts);
                db.Products.RemoveRange(db.Products);
                db.Stores.RemoveRange(db.Stores);
                await db.SaveChangesAsync();

                // Seed a store.
                var store = new Store
                {
                    StoreName = "Test Store",
                    Location = "123 Test Street, London, UK"
                };
                db.Stores.Add(store);
                await db.SaveChangesAsync();

                // Seed a product.
                var product = new Product
                {
                    ProductName = "Test Product",
                    BarcodeId = "1111111111111"
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();

                // Seed a StoreProduct with a price of 10.0.
                var storeProduct = new StoreProduct
                {
                    StoreId = store.StoreId,
                    ProductId = product.ProductId,
                    Price = 10.0m
                };
                db.StoreProducts.Add(storeProduct);
                await db.SaveChangesAsync();

                // Create two receipts for UserId = 1 with different PurchaseDates.
                var receiptRecent = new Receipt
                {
                    StoreId = store.StoreId,
                    UserId = 1,
                    Total = 20.0m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-1) // More recent.
                };
                var receiptOlder = new Receipt
                {
                    StoreId = store.StoreId,
                    UserId = 1,
                    Total = 30.0m,
                    PurchaseDate = DateTime.UtcNow.AddDays(-2) // Less recent.
                };
                db.Receipts.AddRange(receiptRecent, receiptOlder);
                await db.SaveChangesAsync();

                // Add ReceiptItems (for simplicity, one item per receipt).
                var item1 = new ReceiptItem
                {
                    ReceiptId = receiptRecent.ReceiptId,
                    ProductId = product.ProductId,
                    Quantity = 2 // Expected item total: 10.0 * 2 = 20.0.
                };
                var item2 = new ReceiptItem
                {
                    ReceiptId = receiptOlder.ReceiptId,
                    ProductId = product.ProductId,
                    Quantity = 3 // Expected item total: 10.0 * 3 = 30.0.
                };
                db.ReceiptItems.AddRange(item1, item2);
                await db.SaveChangesAsync();
            }

            // Act: Call the GetUserReceipts endpoint for UserId = 1.
            var response = await _client.GetAsync("/api/Receipts/User/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Deserialize the response.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var receipts = await response.Content.ReadFromJsonAsync<List<ReceiptDto>>(options);
            Assert.NotNull(receipts);
            Assert.Equal(2, receipts.Count);

            // Assert that the receipts are sorted descending by PurchaseDate.
            // The more recent receipt (receiptRecent) should be first.
            Assert.True(receipts[0].PurchaseDate > receipts[1].PurchaseDate,
                "Receipts are not sorted descending by PurchaseDate.");
        }

        [Fact]
        public async Task GetReceiptDetails_ReturnsCorrectReceipt()
        {
            int receiptId;
            // Arrange: Seed a receipt with one receipt item.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                // Clear existing data.
                db.Receipts.RemoveRange(db.Receipts);
                db.ReceiptItems.RemoveRange(db.ReceiptItems);
                db.StoreProducts.RemoveRange(db.StoreProducts);
                db.Products.RemoveRange(db.Products);
                db.Stores.RemoveRange(db.Stores);
                await db.SaveChangesAsync();

                // Seed a store.
                var store = new Store
                {
                    StoreName = "Test Store",
                    Location = "123 Test Street, London, UK"
                };
                db.Stores.Add(store);
                await db.SaveChangesAsync();

                // Seed a product.
                var product = new Product
                {
                    ProductName = "Test Product",
                    BarcodeId = "1111111111111"
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();

                // Seed a StoreProduct with a price of 15.0.
                var storeProduct = new StoreProduct
                {
                    StoreId = store.StoreId,
                    ProductId = product.ProductId,
                    Price = 15.0m
                };
                db.StoreProducts.Add(storeProduct);
                await db.SaveChangesAsync();

                // Create a receipt for UserId = 1 with Total = 45.0.
                var receipt = new Receipt
                {
                    StoreId = store.StoreId,
                    UserId = 1,
                    Total = 45.0m,
                    PurchaseDate = DateTime.UtcNow
                };
                db.Receipts.Add(receipt);
                await db.SaveChangesAsync();
                receiptId = receipt.ReceiptId;

                // Add one ReceiptItem: Quantity = 3, so expected ItemTotal = 15.0 * 3 = 45.0.
                var receiptItem = new ReceiptItem
                {
                    ReceiptId = receipt.ReceiptId,
                    ProductId = product.ProductId,
                    Quantity = 3
                };
                db.ReceiptItems.Add(receiptItem);
                await db.SaveChangesAsync();
            }

            // Act: Call the GetReceiptDetails endpoint for the seeded receipt.
            var response = await _client.GetAsync($"/api/Receipts/{receiptId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Deserialize the response.
            var options2 = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var receiptDetails = await response.Content.ReadFromJsonAsync<ReceiptDetailDto>(options2);
            Assert.NotNull(receiptDetails);
            Assert.Equal(receiptId, receiptDetails.ReceiptId);
            Assert.Equal("Test Store", receiptDetails.StoreName);
            Assert.Equal(1, receiptDetails.UserId);
            Assert.Equal(45.0m, receiptDetails.Total);
            Assert.NotNull(receiptDetails.ReceiptItems);
            Assert.Single(receiptDetails.ReceiptItems);

            var item = receiptDetails.ReceiptItems.First();
            Assert.Equal(3, item.Quantity);
            Assert.Equal(15.0m, item.Price);
            Assert.Equal(45.0m, item.ItemTotal);
        }
    }

    // DTO classes to help deserialize the JSON responses.
    public class ReceiptDto
    {
        public int ReceiptId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<ReceiptItemDto> ReceiptItems { get; set; }
    }

    public class ReceiptItemDto
    {
        public int ItemId { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal ItemTotal { get; set; }
    }

    public class ReceiptDetailDto
    {
        public int ReceiptId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<ReceiptItemDto> ReceiptItems { get; set; }
    }
}
