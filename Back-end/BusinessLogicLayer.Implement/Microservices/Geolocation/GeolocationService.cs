using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Geolocation;
using BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SetupLayer.Setting.Microservices.Geolocation;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Microservices.Geolocation
{
    public class GeolocationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, GeolocationSettings geolocationSettings) : IGeolocation
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly GeolocationSettings _geolocationSettings = geolocationSettings;
        // API key từ dịch vụ IPGeolocation hoặc ipstack

        public async Task<GeolocationResponseModel> GetLocationFromHeaderAsync()
        {
            string ip = Util.GetIpAddress();

            var requestUri = $"https://api.ipgeolocation.io/ipgeo?apiKey={_geolocationSettings.ApiKey}&ip={ip}";

            var response = await _httpClient.GetStringAsync(requestUri);
            var locationData = JsonConvert.DeserializeObject<GeolocationResponseModel>(response) ?? throw new DataNotFoundCustomException("Fetch data from Geolocation API failed");

            return locationData;
        }

        public async Task<GeolocationResponseModel> GetLocationFromApiAsync()
        {
            string requestUri = $"https://api.ipgeolocation.io/ipgeo?apiKey={_geolocationSettings.ApiKey}";

            string response = await _httpClient.GetStringAsync(requestUri);
            GeolocationResponseModel locationData = JsonConvert.DeserializeObject<GeolocationResponseModel>(response) ?? throw new DataNotFoundCustomException("Fetch data from Geolocation API failed");
           
            return locationData;
        }

        public async Task<GeolocationResponseModel> GetLocationFromIpAsync(string ip)
        {
            var requestUri = $"https://api.ipgeolocation.io/ipgeo?apiKey={_geolocationSettings.ApiKey}&ip={ip}";

            var response = await _httpClient.GetStringAsync(requestUri);
            var locationData = JsonConvert.DeserializeObject<GeolocationResponseModel>(response) ?? throw new DataNotFoundCustomException("Fetch data from Geolocation API failed");

            return locationData;
        }
    }
}
