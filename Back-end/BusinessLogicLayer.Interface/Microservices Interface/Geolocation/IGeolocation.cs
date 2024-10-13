using BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response;

namespace BusinessLogicLayer.Interface.Microservices_Interface.Geolocation
{
    public interface IGeolocation
    {
        Task<GeolocationResponseModel> GetLocationAsync();
        Task<GeolocationResponseModel> GetLocationFromIpAsync(string ip);
    }
}
