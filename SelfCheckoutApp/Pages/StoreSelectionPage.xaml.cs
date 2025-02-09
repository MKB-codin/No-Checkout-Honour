using System.Net.Http.Json;
using System.Device.Location;
using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json.Linq;

namespace SelfCheckoutApp.Pages
{
    public partial class StoreSelectionPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public StoreSelectionPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            })
            {
                BaseAddress = new Uri("https://192.168.0.41:7249")
            };

            GetUserLocationAndLoadStores();
        }

        private async void GetUserLocationAndLoadStores()
        {
            try
            {
                // Get the user's current location.
                // In production, replace this with a robust geolocation API.
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    // Fallback: use example coordinates if location is unavailable.
                    location = new Location(51.509865, -0.118092); // London coordinates
                }

                await LoadNearestStores(location.Latitude, location.Longitude);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Location Error", $"Error fetching location: {ex.Message}", "OK");
            }
        }

        private async Task LoadNearestStores(double userLatitude, double userLongitude)
        {
            try
            {
                // Request the 3 nearest stores from the server.
                string requestUrl = $"/api/Stores/GetNearestStores?userLatitude={userLatitude}&userLongitude={userLongitude}";
                var stores = await _httpClient.GetFromJsonAsync<List<Store>>(requestUrl);

                if (stores != null && stores.Any())
                {
                    StoresCollectionView.ItemsSource = stores;
                }
                else
                {
                    await DisplayAlert("No Stores", "No nearby stores found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load stores: {ex.Message}", "OK");
            }
        }

        private async void OnStoreButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Store selectedStore)
            {
                bool confirm = await DisplayAlert("Confirm Store",
                    $"You selected {selectedStore.StoreName}. Is this correct?", "Yes", "No");

                if (confirm)
                {
                    // Navigate to the Scan Item Page, passing the selected store.
                    await Navigation.PushAsync(new ScanItemPage(selectedStore));
                }
            }
        }
    }

    public class Store
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
