using SelfCheckoutApp.Services;
using System.Net.Http.Json;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace SelfCheckoutApp.Pages 
{
    public partial class ScanItemPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private bool _isProcessingBarcode = false;
        private readonly UserSession _userSession;
        public ScanItemPage(UserSession userSession)
        {
            InitializeComponent();
            _userSession = userSession;
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            })
            {
                BaseAddress = new Uri("https://192.168.0.41:7249")
            };
            StoreLabel.Text = $"Shopping at {_userSession.StoreName}";
            barcodeReader = this.FindByName<CameraBarcodeReaderView>("barcodeReader");
        }

        private async void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_isProcessingBarcode)
                return; // Prevent multiple detections

            _isProcessingBarcode = true;
            string barcode = e.Results.FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(barcode))
            {
                await ProcessBarcode(barcode);
            }

            _isProcessingBarcode = false;
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            string barcode = BarcodeEntry.Text;

            if (!string.IsNullOrWhiteSpace(barcode))
            {
                await ProcessBarcode(barcode);
            }
        }

        private async Task ProcessBarcode(string barcode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Product>($"/api/Products/GetProductByBarcode/{barcode}");

                if (response != null)
                {
                    bool confirm = await DisplayAlert("Add to Cart",
                        $"Product: {response.ProductName}\nPrice: £{response.Price}\n\nAdd to basket?", "Yes", "No");

                    if (confirm)
                    {
                        // Move to basket logic (to be implemented)
                        await DisplayAlert("Success", $"{response.ProductName} added to cart!", "OK");
                    }
                }
                else
                {
                    ErrorMessage.Text = "Product not found.";
                    ErrorMessage.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error: {ex.Message}";
                ErrorMessage.IsVisible = true;
            }
        }

        private void barcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {

        }

        protected override void OnDisappearing() // Stop camera use when no longer on page
        {
            base.OnDisappearing();

            if (barcodeReader != null)
            {
                barcodeReader.IsDetecting = false; 
                barcodeReader.IsVisible = false;
                barcodeReader.Handler?.DisconnectHandler();
            }
        }
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeId { get; set; }
        public double Price { get; set; }
    }
}
