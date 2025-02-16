using SelfCheckoutApp.Constants;
using SelfCheckoutApp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;

namespace SelfCheckoutApp.Pages
{
    public partial class BasketPage : ContentPage
    {
        private readonly UserSession _userSession;
        private readonly HttpClient _httpClient;

        public ObservableCollection<UserSession.CartItem> CartItems => _userSession.CartItems;

        public BasketPage(UserSession userSession)
        {
            InitializeComponent();
            _userSession = userSession;
            BindingContext = this;

            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(ApiConstants.BaseUri)
            };
            UpdateTotalPrice();
        }

        private void OnIncreaseQuantity(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is UserSession.CartItem item)
            {
                item.ItemQuantity++;
                UpdateTotalPrice();
            }
        }

        private async void OnDecreaseQuantity(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is UserSession.CartItem item)
            {
                if (item.ItemQuantity > 1)
                {
                    item.ItemQuantity--;
                }
                else
                {
                    bool confirm = await DisplayAlert("Confirm Removal",
                        $"Do you really want to remove {item.ItemName} from your basket?", "Yes", "No");
                    if (confirm)
                    {
                        CartItems.Remove(item);
                    }
                }
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            double total = CartItems.Sum(item => item.ItemPrice * item.ItemQuantity);
            TotalPriceLabel.Text = $"Total: £{total:F2}";
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScanItemPage(_userSession));
        }

        private async void OnCheckoutClicked(object sender, EventArgs e)
        {
            if (!CartItems.Any())
            {
                await DisplayAlert("Empty Cart", "Your basket is empty.", "OK");
                return;
            }

            double total = CartItems.Sum(item => item.ItemPrice * item.ItemQuantity);

            // Build your checkout session request
            var checkoutRequest = new
            {
                UserId = _userSession.UserId,
                StoreId = _userSession.StoreId,
                Total = total,
                SuccessUrl = "https://192.168.0.41:7249/api/Payments/payment-success",
                CancelUrl = "https://192.168.0.41:7249/api/Payments/payment-cancel"

            };

            var createResponse = await _httpClient.PostAsJsonAsync("/api/Payment/CreateCheckoutSession", checkoutRequest);
            if (!createResponse.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to create checkout session.", "OK");
                return;
            }

            var result = await createResponse.Content.ReadFromJsonAsync<CreateCheckoutSessionResponse>();
            if (result == null || string.IsNullOrEmpty(result.Url))
            {
                await DisplayAlert("Error", "Failed to get payment URL.", "OK");
                return;
            }

            // Navigate to the PaymentPage with the checkout URL
            await Navigation.PushAsync(new PaymentPage(result.Url, _userSession, total));
        }

        public class CreateCheckoutSessionResponse
        {
            public string Url { get; set; }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateTotalPrice();
        }
    }

    // Response model for PaymentIntent creation
    public class CreatePaymentIntentResponse
    {
        public string ClientSecret { get; set; }
    }

    public class CreateCheckoutSessionResponse
    {
        public string Url { get; set; }
    }
}
