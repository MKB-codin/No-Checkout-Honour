using SelfCheckoutApp.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Linq;

namespace SelfCheckoutApp.Pages
{
    public partial class ReceiptsPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly UserSession _userSession;

        // Observable collection for binding
        public ObservableCollection<ReceiptResponse> Receipts { get; set; } = new ObservableCollection<ReceiptResponse>();

        public ReceiptsPage(UserSession userSession)
        {
            InitializeComponent();
            _userSession = userSession;
            BindingContext = this; // Ensure the BindingContext is set

            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            })
            {
                BaseAddress = new Uri("https://192.168.0.41:7249")
            };

            LoadReceipts();
        }

        private async void LoadReceipts()
        {
            try
            {
                int userId = _userSession.UserId;
                var receipts = await _httpClient.GetFromJsonAsync<List<ReceiptResponse>>($"/api/Receipts/User/{userId}");
                if (receipts != null && receipts.Any())
                {
                    Receipts.Clear();
                    foreach (var receipt in receipts)
                    {
                        Receipts.Add(receipt);
                    }
                }
                else
                {
                    await DisplayAlert("Info", "No receipts found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load receipts: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadReceipts();
        }

        private async void ReceiptsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ReceiptResponse selectedReceipt)
            {
                await Navigation.PushAsync(new ReceiptDetailsPage(selectedReceipt));
            }
        }
    }

    public class ReceiptResponse
    {
        public int ReceiptId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } // NEW PROPERTY
        public int UserId { get; set; }
        public double Total { get; set; }
        public DateTime PurchaseDate { get; set; }
        public List<ReceiptItemResponse> ReceiptItems { get; set; }
    }


    public class ReceiptItemResponse
    {
        public int ItemId { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
