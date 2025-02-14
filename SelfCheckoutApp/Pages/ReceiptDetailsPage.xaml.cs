using SelfCheckoutApp.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace SelfCheckoutApp.Pages
{
    public partial class ReceiptDetailsPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly ReceiptResponse _initialReceipt;

        public ReceiptDetailsPage(ReceiptResponse receipt)
        {
            InitializeComponent();
            _initialReceipt = receipt;

            // Initialize HttpClient with your server's base URL.
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            })
            {
                BaseAddress = new Uri("https://192.168.0.41:7249")
            };

            // Load full receipt details and update the BindingContext.
            LoadReceiptDetails();
        }

        private async void LoadReceiptDetails()
        {
            try
            {
                var receiptDetails = await _httpClient.GetFromJsonAsync<ReceiptResponse>($"/api/Receipts/{_initialReceipt.ReceiptId}");
                if (receiptDetails != null)
                {
                    // Set the BindingContext to the fully loaded receipt details.
                    BindingContext = receiptDetails;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load receipt details.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load receipt details: {ex.Message}", "OK");
            }
        }
    }
}
