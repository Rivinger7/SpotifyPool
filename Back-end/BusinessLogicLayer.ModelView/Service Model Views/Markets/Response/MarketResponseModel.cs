using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Markets.Response
{
	public class MarketResponseModel
    {
        [JsonProperty("markets")]
        public List<string> MarketCode { get; set; } = null!;
    }
}
