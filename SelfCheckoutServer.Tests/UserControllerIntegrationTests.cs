using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using HonourSelfCheckoutServer;
using SelfCheckoutServer.Tests.Factories;

namespace SelfCheckoutServer.Tests
{
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_NewUser_ReturnsSuccess()
        {

            string uniqueEmail = $"user{DateTime.UtcNow.Ticks}@example.com";
            var newUser = new
            {
                name = "Test User",
                email = uniqueEmail,
                phone = "1234567890",
                password = "StrongPassword123",
                hashedemail = ""
            };


            var response = await _client.PostAsJsonAsync("/api/Users/Register", newUser);


            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Registration complete", messageProp.GetString());

            var duplicateResponse = await _client.PostAsJsonAsync("/api/Users/Register", newUser);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

            var duplicateJson = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(duplicateJson.TryGetProperty("message", out JsonElement errormessage));
            Assert.Equal("This email is already in use", errormessage.GetString());
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {

            var loginCredentials = new
            {
                email = "testuser@example.com",
                password = "TestPassword123"
            };


            var response = await _client.PostAsJsonAsync("/api/Users/Login", loginCredentials);


            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Login successful", messageProp.GetString());
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {

            var loginCredentials = new
            {
                email = "testuser@example.com",
                password = "WrongPassword"
            };

            var response = await _client.PostAsJsonAsync("/api/Users/Login", loginCredentials);


            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Invalid email or password", messageProp.GetString());
        }
    }
}
