namespace BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response
{
    public class GeolocationResponseModel
    {
        public string CountryCode2 { get; set; } // Đây là Alpha-2 Code của quốc gia
        public string CountryName { get; set; }
        public string City { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
