using System.Net.Http.Json;
using System.Device.Location;
using Newtonsoft.Json.Linq; 

namespace SelfCheckoutApp.Pages
{
    public partial class StoreSelectionPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private const string GeocodingApiKey = "YOUR_GEOCODING_API_KEY"; // Replace with your API key
        private const string GeocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json"; // Example: Google Maps API

        public StoreSelectionPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://192.168.0.41:7249") }; // Update with your server IP
            LoadStores();
        }

        private async void LoadStores()
        {
            try
            {
                // Fetch stores from server
                var stores = await _httpClient.GetFromJsonAsync<List<Store>>("/api/Stores/GetAllStores");

                // Get user's location (replace with proper geolocation API for production)
                var userLocation = new GeoCoordinate(51.509865, -0.118092); // Example: London coordinates

                // Retrieve latitude and longitude for each store
                foreach (var store in stores)
                {
                    var (latitude, longitude) = await GetCoordinatesForAddress(store.Location);
                    store.Latitude = latitude;
                    store.Longitude = longitude;
                }

                // Sort stores by distance
                stores = stores.OrderBy(store =>
                {
                    var storeLocation = new GeoCoordinate(store.Latitude, store.Longitude);
                    return userLocation.GetDistanceTo(storeLocation);
                }).ToList();

                // Bind to CollectionView
                StoresCollectionView.ItemsSource = stores;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load stores: {ex.Message}", "OK");
            }
        }

        private async Task<(double latitude, double longitude)> GetCoordinatesForAddress(string address)
        {
            try
            {
                // Call the geocoding API
                string url = $"{GeocodingApiUrl}?address={Uri.EscapeDataString(address)}&key={GeocodingApiKey}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    // Parse the latitude and longitude
                    var location = jsonObject["results"]?[0]?["geometry"]?["location"];
                    double latitude = location?["lat"]?.Value<double>() ?? 0;
                    double longitude = location?["lng"]?.Value<double>() ?? 0;

                    return (latitude, longitude);
                }

                throw new Exception("Failed to fetch coordinates.");
            }
            catch
            {
                // Return default coordinates if the address cannot be resolved
                return (0, 0);
            }
        }

        private async void OnStoreSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedStore = e.CurrentSelection.FirstOrDefault() as Store;

            if (selectedStore != null)
            {
                bool confirm = await DisplayAlert("Confirm Store",
                    $"You selected {selectedStore.StoreName}. Is this correct?", "Yes", "No");

                if (confirm)
                {
                    // Navigate to the next step (e.g., scan item page)
                    //await Navigation.PushAsync(new ScanItemPage(selectedStore));
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
