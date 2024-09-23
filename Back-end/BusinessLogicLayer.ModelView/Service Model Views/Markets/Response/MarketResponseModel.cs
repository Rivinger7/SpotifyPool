using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Markets.Response
{
    public class MarketResponseModel
    {
        [JsonProperty("markets")]
        public List<string> MarketCode { get; set; } = null!;
    }
}
