using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using SelfCheckoutServer.Tests.Factories;
using Xunit;
using HonourSelfCheckoutServer; // Adjust to your server project's namespace

namespace SelfCheckoutServer.Tests
{
    public class PaymentControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PaymentControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreatePaymentIntent_WithValidData_ReturnsClientSecret()
        {
            // Arrange: Create a checkout request payload.
            var checkoutRequest = new
            {
                UserId = 1,
                StoreId = 1,
                Total = 12.34
            };

            // Act: Send a POST request to create a Payment Intent.
            var response = await _client.PostAsJsonAsync("/api/Payment/CreatePaymentIntent", checkoutRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert: Verify that the response contains a non-empty client secret.
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string clientSecret;
            if (json.TryGetProperty("clientSecret", out JsonElement prop))
            {
                clientSecret = prop.GetString();
            }
            else if (json.TryGetProperty("ClientSecret", out prop))
            {
                clientSecret = prop.GetString();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'clientSecret' property.");
            }
            Assert.False(string.IsNullOrEmpty(clientSecret));
        }

        [Fact]
        public async Task FinalizeCheckout_WithValidData_ReturnsCheckoutSuccessful()
        {
            // Arrange: Create a checkout finalization payload.
            var checkoutFinalizationRequest = new
            {
                UserId = 1,
                StoreId = 1,
                Total = 12.34,
                CartItems = new[]
                {
                    new { ProductId = 1, Quantity = 2 }
                }
            };

            // Act: Send a POST request to finalize the checkout.
            var response = await _client.PostAsJsonAsync("/api/Payment/FinalizeCheckout", checkoutFinalizationRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert: Verify that the response contains a success message and a valid ReceiptId.
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string message;
            if (json.TryGetProperty("message", out JsonElement prop))
            {
                message = prop.GetString();
            }
            else if (json.TryGetProperty("Message", out prop))
            {
                message = prop.GetString();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'message' property.");
            }
            Assert.Equal("Checkout successful", message);

            int receiptId;
            if (json.TryGetProperty("ReceiptId", out JsonElement idProp))
            {
                receiptId = idProp.GetInt32();
            }
            else if (json.TryGetProperty("receiptId", out idProp))
            {
                receiptId = idProp.GetInt32();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'ReceiptId' property.");
            }
            Assert.True(receiptId > 0);
        }

        [Fact]
        public async Task CreateCheckoutSession_WithValidData_ReturnsUrl()
        {
            // Arrange: Create a checkout session request payload.
            var checkoutSessionRequest = new
            {
                UserId = 1,
                StoreId = 1,
                Total = 12.34,
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };

            // Act: Send a POST request to create a checkout session.
            var response = await _client.PostAsJsonAsync("/api/Payment/CreateCheckoutSession", checkoutSessionRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert: Verify that the response contains a non-empty URL.
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string url;
            if (json.TryGetProperty("url", out JsonElement prop))
            {
                url = prop.GetString();
            }
            else if (json.TryGetProperty("Url", out prop))
            {
                url = prop.GetString();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'url' property.");
            }
            Assert.False(string.IsNullOrEmpty(url));
        }

        [Fact]
        public async Task PaymentSuccessEndpoint_ReturnsSuccessMessage()
        {
            // Act: Send a GET request to the payment-success endpoint.
            var response = await _client.GetAsync("/api/Payment/payment-success");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert: Verify that the response message is as expected.
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string message;
            if (json.TryGetProperty("message", out JsonElement prop))
            {
                message = prop.GetString();
            }
            else if (json.TryGetProperty("Message", out prop))
            {
                message = prop.GetString();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'message' property.");
            }
            Assert.Equal("Payment successful! Your receipt has been generated.", message);
        }

        [Fact]
        public async Task PaymentCancelEndpoint_ReturnsCancelMessage()
        {
            // Act: Send a GET request to the payment-cancel endpoint.
            var response = await _client.GetAsync("/api/Payment/payment-cancel");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert: Verify that the response message is as expected.
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            string message;
            if (json.TryGetProperty("message", out JsonElement prop))
            {
                message = prop.GetString();
            }
            else if (json.TryGetProperty("Message", out prop))
            {
                message = prop.GetString();
            }
            else
            {
                throw new System.Exception("Response did not contain a 'message' property.");
            }
            Assert.Equal("Payment cancelled. No charges were made.", message);
        }
    }
}
