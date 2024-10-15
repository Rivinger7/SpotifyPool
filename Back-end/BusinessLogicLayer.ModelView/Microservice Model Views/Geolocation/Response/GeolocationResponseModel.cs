using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Microservice_Model_Views.Geolocation.Response
{
    public class GeolocationResponseModel
    {
        [JsonProperty("ip")]
        public string? IP { get; set; }

        [JsonProperty("hostname")]
        public string? HostName { get; set; }

        [JsonProperty("continent_code")]
        public string? ContinentCode { get; set; }

        [JsonProperty("continent_name")]
        public string? ContinentName { get; set; }

        [JsonProperty("country_code2")]
        public string? CountryCode2 { get; set; }

        [JsonProperty("country_code3")]
        public string? CountryCode3 { get; set; }

        [JsonProperty("country_name")]
        public string? CountryName { get; set; }

        [JsonProperty("country_capital")]
        public string? CountryCapital { get; set; }

        [JsonProperty("state_prov")]
        public string? StateProvince { get; set; }

        [JsonProperty("district")]
        public string? District { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("zipcode")]
        public string? ZipCode { get; set; }

        [JsonProperty("latitude")]
        public float? Latitude { get; set; }

        [JsonProperty("longitude")]
        public float? Longitude { get; set; }

        [JsonProperty("is_eu")]
        public bool IsEU { get; set; }

        [JsonProperty("calling_code")]
        public string? CallingCode { get; set; }

        [JsonProperty("country_tld")]
        public string? CountryTLD { get; set; }

        [JsonProperty("languages")]
        public string? Languages { get; set; }

        [JsonProperty("country_flag")]
        public string? CountryFlag { get; set; }

        [JsonProperty("isp")]
        public string? ISP { get; set; }

        [JsonProperty("connection_type")]
        public string? ConnectionType { get; set; }

        [JsonProperty("organization")]
        public string? Organization { get; set; }

        [JsonProperty("asn")]
        public string? ASN { get; set; }

        [JsonProperty("geoname_id")]
        public int GeoNameID { get; set; }

        [JsonProperty("currency")]
        public Currency? Currency { get; set; }

        [JsonProperty("time_zone")]
        public TimeZone? TimeZone { get; set; }
    }

    public class Currency
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
    }

    public class TimeZone
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }

        [JsonProperty("current_time")]
        public string? CurrentTime { get; set; }

        [JsonProperty("current_time_unix")]
        public double? CurrentTimeUnix { get; set; }

        [JsonProperty("is_dst")]
        public bool? IsDST { get; set; }

        [JsonProperty("dst_savings")]
        public int? DstSavings { get; set; }
    }

}
