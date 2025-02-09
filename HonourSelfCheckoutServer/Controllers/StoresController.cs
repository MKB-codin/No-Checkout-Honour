using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HonourSelfCheckoutServer.Data;
using HonourSelfCheckoutServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using Newtonsoft.Json.Linq;

namespace HonourSelfCheckoutServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private const string GoogleGeocodingApiKey = "AIzaSyBvR3a8ZinM40HwLm7hp2mEX2hPTGlDERQ";
        // Base URL for Google Geocoding API
        private const string GeocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";

        public StoresController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("GetAllStores")]
        public async Task<IActionResult> GetAllStores()
        {
            try
            {
                var stores = await _databaseContext.Stores.ToListAsync();
                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving stores.", Error = ex.Message });
            }
        }

        // New endpoint: Returns the 3 nearest stores based on user's latitude and longitude.
        [HttpGet("GetNearestStores")]
        public async Task<IActionResult> GetNearestStores([FromQuery] double userLatitude, [FromQuery] double userLongitude)
        {
            try
            {
                // Fetch all stores from the database (each store has StoreId, StoreName, and Location)
                var stores = await _databaseContext.Stores.ToListAsync();

                // Prepare an HttpClient for geocoding (ideally reuse one; here we create a new one)
                using (var httpClient = new HttpClient())
                {
                    List<StoreDTO> storeDTOs = new List<StoreDTO>();

                    // For each store, get its latitude and longitude using the Google Geocoding API
                    foreach (var store in stores)
                    {
                        var (latitude, longitude) = await GetCoordinatesForAddress(store.Location, httpClient);
                        storeDTOs.Add(new StoreDTO
                        {
                            StoreId = store.StoreId,
                            StoreName = store.StoreName,
                            Location = store.Location,
                            Latitude = latitude,
                            Longitude = longitude
                        });
                    }

                    // Compute distances from the user's location using GeoCoordinate
                    var userCoordinate = new GeoCoordinate(userLatitude, userLongitude);
                    var nearestStores = storeDTOs
                        .OrderBy(s => userCoordinate.GetDistanceTo(new GeoCoordinate(s.Latitude, s.Longitude)))
                        .Take(3)
                        .ToList();

                    return Ok(nearestStores);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving nearest stores", Error = ex.Message });
            }
        }

        // Helper method to call the Google Geocoding API and return coordinates for a given address.
        private async Task<(double latitude, double longitude)> GetCoordinatesForAddress(string address, HttpClient httpClient)
        {
            try
            {
                string url = $"{GeocodingApiUrl}?address={Uri.EscapeDataString(address)}&key={GoogleGeocodingApiKey}";
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to contact the Geocoding API.");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);
                var results = jsonObject["results"];
                if (results == null || !results.Any())
                {
                    throw new Exception("No valid location data returned.");
                }

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
                // In case of error, you can log it and return default coordinates (0,0)
                return (0, 0);
            }
        }
    }

    // DTO class for returning store data with computed coordinates.
    public class StoreDTO
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
