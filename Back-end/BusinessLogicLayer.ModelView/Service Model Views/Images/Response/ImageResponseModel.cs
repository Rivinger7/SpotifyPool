﻿using Newtonsoft.Json;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response
{
    public class ImageResponseModel
    {
        [JsonProperty("url")]
        public string URL { get; set; } = null!;

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
