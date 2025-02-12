using System.Net.Http.Json;
using ZXing.Net.Maui;
using SelfCheckoutApp.Services;
using System.Linq;
using System.Threading.Tasks;
using ZXing;

namespace SelfCheckoutApp.Pages
{
    public partial class ScanItemPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;
        private bool _isProcessingBarcode = false;

        // Constructor now accepts a UserSession.
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

            // If a store has been selected, display its name.
            if (_userSession.StoreName != null)
                StoreLabel.Text = $"Shopping at {_userSession.StoreName}";
            else
            {
                Navigation.PopAsync(); //If they somehow manage to get to the scan item page without choosing a store, this should send them back to select a store.
            }
        }

        private async void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_isProcessingBarcode)
                return;

            _isProcessingBarcode = true;

            // Disable scanning immediately to prevent repeated events.
            barcodeReader.IsDetecting = false;

            var barcodeResult = e.Results.FirstOrDefault();
            if (barcodeResult != null && !string.IsNullOrEmpty(barcodeResult.Value))
            {

                var allowedFormats = new List<ZXing.Net.Maui.BarcodeFormat>
                {
                    ZXing.Net.Maui.BarcodeFormat.Ean13,
                    ZXing.Net.Maui.BarcodeFormat.Ean8,
                    ZXing.Net.Maui.BarcodeFormat.UpcA,
                    ZXing.Net.Maui.BarcodeFormat.UpcE,
                    ZXing.Net.Maui.BarcodeFormat.Code128
                };

                if (!allowedFormats.Contains(barcodeResult.Format))
                {
                    // Use the main thread for UI updates.
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Invalid Barcode", "This barcode format is not supported. Please scan a valid product barcode.", "OK");
                        _isProcessingBarcode = false;
                        return;
                    });
                }
                else
                {
                    // Use the main thread for UI updates.
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await ProcessBarcode(barcodeResult.Value);
                    });
                }
            }

            // Wait for a short period to avoid re-triggering immediately.
            await Task.Delay(500);

            // Re-enable detection if the page is still visible.
            if (this.IsVisible)
            {
                barcodeReader.IsDetecting = true;
            }

            _isProcessingBarcode = false;
        }

        private async Task ProcessBarcode(string barcode)
        {
            try
            {
                // Call the API endpoint to retrieve product details by barcode.
                var product = await _httpClient.GetFromJsonAsync<Product>($"/api/Products/{barcode}");
                if (product != null)
                {
                    bool addToCart = await DisplayAlert("Add to Cart",
                        $"Product: {product.ProductName}\nPrice: £{product.Price:F2}\n\nAdd to basket?", "Yes", "No");
                    if (addToCart)
                    {
                        // Create a new CartItem and add it to the session's cart.
                        var newItem = new Services.UserSession.CartItem { ItemName = product.ProductName, ItemPrice = product.Price, ItemQuantity = 1 };
                        _userSession.CartItems.Add(newItem);

                        await DisplayAlert("Success", $"{product.ProductName} added to cart!", "OK");

                        // Navigate back to the Basket page.
                        await Navigation.PopAsync();
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
    }


    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeId { get; set; }
        public double Price { get; set; }
    }
}
