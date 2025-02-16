using Microsoft.Maui.Controls;
using SelfCheckoutApp.Constants;
using SelfCheckoutApp.Services;
using System.Net.Http.Json;

namespace SelfCheckoutApp.Pages
{
    public partial class PaymentPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;
        private readonly double _total;

        public PaymentPage(string checkoutUrl, UserSession userSession, double total)
        {
            InitializeComponent();

            // We'll need the user session to finalize checkout and clear the cart
            _userSession = userSession;
            _total = total;

            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(ApiConstants.BaseUri)
            };

            paymentWebView.Source = checkoutUrl;
        }

        private async void PaymentWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url.Contains("payment-success"))
            {
                await FinalizeCheckout();
                await DisplayAlert("Payment", "Payment successful!", "OK");
                await Navigation.PopAsync();
            }
            else if (e.Url.Contains("payment-cancel"))
            {
                await DisplayAlert("Payment", "Payment was cancelled.", "OK");
                await Navigation.PopAsync();
            }
        }

        private async Task FinalizeCheckout()
        {
            // Prepare finalization request with cart details
            var finalizeRequest = new
            {
                UserId = _userSession.UserId,
                StoreId = _userSession.StoreId,
                Total = _total,
                CartItems = _userSession.CartItems.Select(item => new
                {
                    ProductId = item.ProductId,
                    Quantity = item.ItemQuantity
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Payment/FinalizeCheckout", finalizeRequest);
            if (response.IsSuccessStatusCode)
            {
                // Clear the cart once server finalization succeeds
                _userSession.CartItems.Clear();
            }
            else
            {
                await DisplayAlert("Error", "Checkout finalization failed.", "OK");
            }
        }
    }
}
