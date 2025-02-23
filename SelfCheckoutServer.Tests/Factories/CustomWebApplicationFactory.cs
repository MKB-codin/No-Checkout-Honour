using System;
using System.Linq;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using HonourSelfCheckoutServer.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;

namespace SelfCheckoutServer.Tests.Factories
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
         where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Force the environment to be "Testing"
            builder.UseEnvironment("Testing");
            System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

            // Override service registrations for testing.
            builder.ConfigureTestServices(services =>
            {

                var dbContextOptionsDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>))
                    .ToList();
                foreach (var descriptor in dbContextOptionsDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Register the DatabaseContext using the InMemory provider.
                services.AddDbContext<DatabaseContext>(options =>
                {
                    // Use a unique database name for isolation.
                    options.UseInMemoryDatabase("TestDatabase_");
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    // Ensure a fresh database each time.
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    SeedTestData(db);
                }
            });
        }

        private void SeedTestData(DatabaseContext db)
        {
            if (!db.Stores.Any())
            {
                var store = new Store
                {
                    StoreName = "Test Store",
                    Location = "123 Test Street"
                };
                db.Stores.Add(store);
                db.SaveChanges();  // Ensure store ID is generated

                var product = new Product
                {
                    ProductName = "Test Product",
                    BarcodeId = "1111111111111"
                };
                db.Products.Add(product);
                db.SaveChanges();  // Ensure product ID is generated

                var storeProduct = new StoreProduct
                {
                    StoreId = store.StoreId,
                    ProductId = product.ProductId,
                    Price = 9.99m
                };
                db.StoreProducts.Add(storeProduct);

                string email = "testuser@example.com";
                string hashedPassword = HashHelper.Hash("TestPassword123"); // Hash the password for testing

                var user = new User
                {
                    Name = EncryptionHelper.Encrypt("Test User"),
                    Email = EncryptionHelper.Encrypt(email),
                    Phone = EncryptionHelper.Encrypt("1234567890"),
                    Password = hashedPassword,
                    HashedEmail = HashHelper.Hash(email) // Store hashed email for lookups
                };

                db.Users.Add(user);
                db.SaveChanges(); // Ensure UserId is generated
            }
        }


    }
}