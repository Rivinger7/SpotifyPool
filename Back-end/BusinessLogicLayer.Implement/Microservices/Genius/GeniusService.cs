using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.Genius;
using DataAccessLayer.Interface.MongoDB.UOW;
using HtmlAgilityPack;
using SetupLayer.Setting.Microservices.Genius;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.Microservices.Genius
{
    public class GeniusService(GeniusSettings geniusSettings) : IGenius
    {
        private readonly string CLIENT_ID = geniusSettings.ClientId;
        private readonly string CLIENT_SECRET = geniusSettings.ClientSecret;
        private readonly string REDIRECT_URI = geniusSettings.RedirectUri;
        private readonly string STATE = geniusSettings.State;

        public string Authorize()
        {
            // Authorize Endpoint chính thức của Genius
            // OAuth 2.0
            const string AUTHORIZATION_ENDPOINT = "https://api.genius.com/oauth/authorize";

            // Authorize Code để trao đổi lấy Access Token và Refresh Token
            const string RESPONSE_TYPE = "code";

            // Các scope để cho phép người yêu cầu truy cập vào các API nhất định của Genius
            const string SCOPE = "me";

            // Xây dựng URL ủy quyền với nhiều scope
            string authorizeUrl = $"{AUTHORIZATION_ENDPOINT}?client_id={CLIENT_ID}&redirect_uri={REDIRECT_URI}&scope={Uri.EscapeDataString(SCOPE)}&state={STATE}&response_type={RESPONSE_TYPE}";

            return authorizeUrl;
        }

        public async Task<string> GetAccessToken(string authorizationCode)
        {
            // Token Endpoint chính thức của Genius
            // OAuth 2.0
            const string TOKEN_ENDPOINT = "https://api.genius.com/oauth/token";

            // Xây dựng request body để trao đổi lấy Access Token
            using HttpClient client = new();
            FormUrlEncodedContent requestBody = new(new Dictionary<string, string>
            {
                { "code", authorizationCode },
                { "client_id", CLIENT_ID },
                { "client_secret", CLIENT_SECRET },
                { "redirect_uri", REDIRECT_URI },
                { "response_type", "code" },
                { "grant_type", "authorization_code" }
            });

            // Gửi request lấy Access Token
            HttpResponseMessage response = await client.PostAsync(TOKEN_ENDPOINT, requestBody);
            response.EnsureSuccessStatusCode();

            // Đọc và trả về Access Token
            string responseContent = await response.Content.ReadAsStringAsync();

            // Parse chuỗi Response sang JSON
            JsonDocument tokenResponse = JsonDocument.Parse(responseContent);

            // Lấy Access Token từ JSON
            string accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString() ?? throw new DataNotFoundCustomException("Access Token is not found");

            return accessToken;
        }

        public async Task<string?> GetUrlLyricsAsync(string accessToken, string trackName, string artistName)
        {
            // Query tìm kiếm trên Genius
            string query = $"{trackName} {artistName}";

            // URI của Genius
            string uri = $"https://api.genius.com/search?q={Uri.EscapeDataString(query)}";

            // Gửi request lấy thông tin từ Genius API
            string response = await GetResponseAync(uri, accessToken);

            // Parse chuỗi Response sang JSON
            JsonDocument searchResponse = JsonDocument.Parse(response);

            // Lấy URL lyrics từ kết quả đầu tiên
            JsonElement hit = searchResponse.RootElement.GetProperty("response")
                                                .GetProperty("hits")
                                                .EnumerateArray()
                                                .FirstOrDefault();

            if (hit.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            string? lyricsUrl = hit.GetProperty("result").GetProperty("url").GetString();

            // Nếu có URL lyrics
            // Sử dụng Web Scraping để lấy lyrics từ trang Genius
            if (!string.IsNullOrEmpty(lyricsUrl))
            {
                return await ScrapeLyricsFromGeniusPageAsync(lyricsUrl);
            }

            return null;
        }

        private static async Task<string?> ScrapeLyricsFromGeniusPageAsync(string lyricsUrl)
        {
            using HttpClient _httpClient = new();
            var response = await _httpClient.GetStringAsync(lyricsUrl);
            var document = new HtmlDocument();
            document.LoadHtml(response);

            var lyricsDiv = document.DocumentNode.SelectSingleNode("//div[@class='lyrics']") ??
                            document.DocumentNode.SelectSingleNode("//div[contains(@class, 'Lyrics__Container')]");

            string? lyrics = lyricsDiv?.InnerText?.Trim();

            if(lyrics is null)
            {
                return null;
            }

            // Giải mã HTML nếu chuỗi không null
            return WebUtility.HtmlDecode(lyrics);
        }

        private static async Task<string> GetResponseAync(string uri, string accessToken)
        {
            // Tạo HTTP Client
            using HttpClient client = new();

            // Thêm Access Token vào Header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Gửi request lấy thông tin từ Genius API
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            // Đọc content của response
            string responseBody = await response.Content.ReadAsStringAsync();

            // Trả về content
            return responseBody;
        }
    }
}
