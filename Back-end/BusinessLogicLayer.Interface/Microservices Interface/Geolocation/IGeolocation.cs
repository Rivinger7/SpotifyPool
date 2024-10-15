using BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response;

namespace BusinessLogicLayer.Interface.Microservices_Interface.Geolocation
{
    public interface IGeolocation
    {
        Task<GeolocationResponseModel> GetLocationFromHeaderAsync();
        Task<GeolocationResponseModel> GetLocationFromApiAsync();
        Task<GeolocationResponseModel> GetLocationFromIpAsync(string ip);
    }
}
