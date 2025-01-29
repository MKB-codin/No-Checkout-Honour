using System.Net.Http.Json;
using System.Device.Location;
using Newtonsoft.Json.Linq; 

namespace SelfCheckoutApp.Pages
{
    public partial class StoreSelectionPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private const string GeocodingApiKey = "AIzaSyBvR3a8ZinM40HwLm7hp2mEX2hPTGlDERQ";
        private const string GeocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json"; // Example: Google Maps API

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
            LoadStores();
        }

        private async void LoadStores()
        {
            try
            {
                // Fetch stores from server
                var stores = await _httpClient.GetFromJsonAsync<List<Store>>("/api/Stores/GetAllStores");
                var userLocation = new GeoCoordinate(51.509865, -0.118092); // Obviously fake coords for security. :]

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
                string url = $"{GeocodingApiUrl}?address={Uri.EscapeDataString(address)}&key={GeocodingApiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to contact the Geocoding API.");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);

                // Check if there are valid results
                var results = jsonObject["results"];
                if (results == null || !results.Any())
                {
                    throw new Exception("No valid location data returned.");
                }

                // Extract latitude and longitude safely
                var location = results[0]?["geometry"]?["location"];
                if (location == null)
                {
                    throw new Exception("Location data is missing.");
                }

                double latitude = location["lat"]?.Value<double>() ?? 0;
                double longitude = location["lng"]?.Value<double>() ?? 0;

                return (latitude, longitude);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Geocoding Error", $"Failed to fetch coordinates: {ex.Message}", "OK");
                return (0, 0);
            }
        }



        private async void OnStoreTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Store selectedStore)
            {
                bool confirm = await DisplayAlert("Confirm Store",
                    $"You selected {selectedStore.StoreName}. Is this correct?", "Yes", "No");

                if (confirm)
                {
                    Console.WriteLine($"Navigating to ScanItemPage with {selectedStore.StoreName}...");

                    // Navigate to the next step (scan item page)
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
