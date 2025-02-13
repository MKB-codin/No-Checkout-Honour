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
            // Add a delay to throttle requests
            await Task.Delay(500);

            try
            {
                int storeId = _userSession.StoreId;
                var response = await _httpClient.GetAsync($"/api/Products/GetProductByBarcode/{barcode}?storeId={storeId}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await ShowErrorMessage("Product not found for the selected store. \n Please scan products from this store");
                    return;
                }

                response.EnsureSuccessStatusCode();
                var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
                if (product != null)
                {
                    bool addToCart = await DisplayAlert("Add to Cart",
                        $"Product: {product.ProductName}\nPrice: £{product.Price:F2}\n\nAdd to basket?", "Yes", "No");
                    if (addToCart)
                    {
                        var existingItem = _userSession.CartItems.FirstOrDefault(i => i.ItemName == product.ProductName);
                        if (existingItem != null)
                        {
                            // Increase the quantity of the existing item
                            existingItem.ItemQuantity++;
                        }
                        else
                        {
                            // Create a new CartItem and add it to the session's cart
                            var newItem = new Services.UserSession.CartItem
                            {
                                ItemName = product.ProductName,
                                ItemPrice = (double)product.Price,
                                ItemQuantity = 1
                            };
                            _userSession.CartItems.Add(newItem);
                        }

                        await DisplayAlert("Success", $"{product.ProductName} added to cart!", "OK");
                        await Navigation.PopAsync();
                    }
                }
                else
                {
                    await ShowErrorMessage("Product not found.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error: {ex.Message}");
            }
        }

        private async Task ShowErrorMessage(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.IsVisible = true;
            await Task.Delay(3000); // wait 3 seconds
            ErrorMessage.IsVisible = false;
        }


    }


    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeId { get; set; }
        public decimal Price { get; set; }
        public int StoreId { get; set; }
    }
}
