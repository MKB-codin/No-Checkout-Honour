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
            // Arrange: Generate a unique email for testing.
            string uniqueEmail = $"user{DateTime.UtcNow.Ticks}@example.com";
            var newUser = new
            {
                name = "Test User",
                email = uniqueEmail,
                phone = "1234567890",
                password = "StrongPassword123",
                hashedemail = ""
            };

            // Act: Send the request to register a new user.
            var response = await _client.PostAsJsonAsync("/api/Users/Register", newUser);

            // Assert: Check if the request was successful.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Registration complete", messageProp.GetString());

            // Verify user exists by attempting to register the same user again (should fail).
            var duplicateResponse = await _client.PostAsJsonAsync("/api/Users/Register", newUser);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);

            var duplicateJson = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(duplicateJson.TryGetProperty("message", out JsonElement errormessage));
            Assert.Equal("This email is already in use", errormessage.GetString());
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange: Use the seeded user from the custom factory.
            // The SeedTestData method in the factory creates a user with:
            // Email: "testuser@example.com" and Password: "TestPassword123"
            var loginCredentials = new
            {
                email = "testuser@example.com",
                password = "TestPassword123"
            };

            // Act: Send the request to login.
            var response = await _client.PostAsJsonAsync("/api/Users/Login", loginCredentials);

            // Assert: Verify the response is successful.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Login successful", messageProp.GetString());
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange: Use incorrect password for the seeded user.
            var loginCredentials = new
            {
                email = "testuser@example.com",
                password = "WrongPassword"
            };

            // Act: Attempt to login with invalid credentials.
            var response = await _client.PostAsJsonAsync("/api/Users/Login", loginCredentials);


            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("message", out JsonElement messageProp));
            Assert.Equal("Invalid email or password", messageProp.GetString());
        }
    }
}
