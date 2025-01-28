using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BACKEND.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public GeocodingService(IHttpClientFactory httpClientFactory, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Mapbox access token is required", nameof(accessToken));

            _accessToken = accessToken;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.mapbox.com/geocoding/v5/");
        }

        public async Task<string?> GetCoordinatesAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be null or empty", nameof(location));

            var url = $"mapbox.places/{Uri.EscapeDataString(location)}.json?access_token={_accessToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error fetching geolocation data: {response.ReasonPhrase}");

            var responseContent = await response.Content.ReadAsStringAsync();
            var locationResponse = JsonSerializer.Deserialize<MapboxGeocodingResponse>(responseContent);

            var coordinates = locationResponse?.Features?.FirstOrDefault()?.Geometry?.Coordinates;
            return coordinates != null ? string.Join(",", coordinates) : null;
        }
    }

    public class MapboxGeocodingResponse
    {
        public Feature[]? Features { get; set; }
    }
    public class Feature
    {
        public BACKEND.Services.Geometry? Geometry { get; set; }
    }

    public class Geometry
    {
        public double[]? Coordinates { get; set; }
    }

}
