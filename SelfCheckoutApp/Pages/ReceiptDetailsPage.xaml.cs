using SelfCheckoutApp.Constants;
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
            _httpClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri(ApiConstants.BaseUri)
            };

            LoadReceiptDetails();
        }

        private async void LoadReceiptDetails()
        {
            try
            {
                var receiptDetails = await _httpClient.GetFromJsonAsync<ReceiptResponse>($"/api/Receipts/{_initialReceipt.ReceiptId}");
                if (receiptDetails != null)
                {

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
