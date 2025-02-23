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
    public class StoresControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public StoresControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task GetAllStores_ReturnsStores()
        {

            var response = await _client.GetAsync("/api/Stores/GetAllStores");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stores = await response.Content.ReadFromJsonAsync<List<Store>>();
            Assert.NotNull(stores);
            Assert.True(stores.Count > 0, "Expected at least one store from seeded data.");
        }


        [Fact]
        public async Task GetNearestStores_ReturnsStoresSortedByDistance()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                db.Stores.RemoveRange(db.Stores);
                await db.SaveChangesAsync();
                db.Stores.AddRange(new List<Store>
                {
                    new Store
                    {
                        StoreName = "The British Museum",
                        Location = "Great Russell St, Bloomsbury, London WC1B 3DG, UK"
                    },
                    new Store
                    {
                        StoreName = "Tower Bridge",
                        Location = "Tower Bridge Rd, London SE1 2UP, UK"
                    },
                    new Store
                    {
                        StoreName = "Buckingham Palace",
                        Location = "Westminster, London SW1A 1AA, UK"
                    }
                });
                await db.SaveChangesAsync();
            }


            double userLatitude = 51.5014;
            double userLongitude = -0.1419;
            var url = $"/api/Stores/GetNearestStores?userLatitude={userLatitude}&userLongitude={userLongitude}";


            var response = await _client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);


            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var storeDTOs = await response.Content.ReadFromJsonAsync<List<StoreDTO>>(options);
            Assert.NotNull(storeDTOs);
            Assert.Equal(3, storeDTOs.Count);

            Assert.Equal("Buckingham Palace", storeDTOs[0].StoreName);
            Assert.Equal("The British Museum", storeDTOs[1].StoreName);
            Assert.Equal("Tower Bridge", storeDTOs[2].StoreName);
        }
    }
}
