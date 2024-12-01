using AutoMapper;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using HtmlAgilityPack;
using MongoDB.Driver;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using MongoDB.Bson;

namespace BusinessLogicLayer.Implement.Services.Tests
{
    public class TestBLL(IUnitOfWork unitOfWork, IMapper mapper, HttpClient httpClient)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly HttpClient _httpClient = httpClient;

        public async Task TestTopTrack(string trackId){
            // Lấy thông tin user từ Context
            string? userId = "6736c563216626b7bf5f1441";

			string? topItemId = await _unitOfWork.GetCollection<TopTrack>()
                                                 .Find(topItem => topItem.UserId == userId) //&& topItem.TrackInfo.Any(track => track.TrackId == trackId))
	                                             .Project(topItem => topItem.TopTrackId)
	                                             .FirstOrDefaultAsync();

			if (topItemId is null)
            {

				// create new
				TopTrack newTopItem = new ()
				{
					UserId = userId,
					TrackInfo =
					[
						new TopTracksInfo
						{
							TrackId = trackId,
							StreamCount = 1
						}
					],
				};
				await _unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newTopItem);
				return;
			}


            TopTracksInfo? trackInfo = await _unitOfWork.GetCollection<TopTrack>()
                                             .Find(topItem => topItem.TopTrackId == topItemId && topItem.TrackInfo.Any(track => track.TrackId == trackId))
                                             .Project(topItem => topItem.TrackInfo.FirstOrDefault(track => track.TrackId == trackId))
                                             .FirstOrDefaultAsync();

            if (trackInfo is null){
                // add new track
                FilterDefinition<TopTrack> addTrackInfofilter = Builders<TopTrack>.Filter.Eq(topItem => topItem.TopTrackId, topItemId);

                UpdateDefinition<TopTrack> addTrackInfoUpdateDefinition = Builders<TopTrack>.Update.Push(topItem => topItem.TrackInfo, new TopTracksInfo
                {
                    TrackId = trackId,
                    StreamCount = 1
                });
                UpdateOptions addTrackInfoUpdateOptions = new() { IsUpsert = false };

                await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(addTrackInfofilter, addTrackInfoUpdateDefinition, addTrackInfoUpdateOptions);
                return;
            }

            var filter = Builders<TopTrack>.Filter.And(
                Builders<TopTrack>.Filter.Eq(topItem => topItem.TopTrackId, topItemId),
                Builders<TopTrack>.Filter.ElemMatch(topItem => topItem.TrackInfo, track => track.TrackId == trackId)
            );

            var updateDefinition = Builders<TopTrack>.Update.Inc("TrackInfo.$.StreamCount", 1);
            var updateOptions = new UpdateOptions { IsUpsert = false };

            UpdateResult trackUpdateResult = await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(filter, updateDefinition, updateOptions);
		
        }

        public async Task SetLyricsToDatabase()
        {
            IEnumerable<Track> tracks = await _unitOfWork.GetCollection<Track>().FindAsync(Builders<Track>.Filter.Empty).Result.ToListAsync();

            foreach (Track track in tracks)
            {
                track.Lyrics = null;
                await _unitOfWork.GetCollection<Track>().ReplaceOneAsync(Builders<Track>.Filter.Eq(t => t.Id, track.Id), track);
            }
        }

        public async Task<string?> GetLyricsAsync(string trackName, string artistName)
        {
            // Access Token của Genius API tạm thời
            string _geniusAccessToken = "MWyKm7Q3lMIIsmjiB0YJi5BBnYFH36lpaa-l20N8WLcVorn9kbIJkF57PGRyGGWB";

            // Tạo URL tìm kiếm trên Genius
            var query = $"{trackName} {artistName}";
            var url = $"https://api.genius.com/search?q={Uri.EscapeDataString(query)}";

            // Thiết lập header với access token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _geniusAccessToken);
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            // Lấy URL lyrics từ kết quả đầu tiên
            var hit = json.RootElement.GetProperty("response")
                                      .GetProperty("hits")
                                      .EnumerateArray()
                                      .FirstOrDefault();

            if (hit.ValueKind == JsonValueKind.Undefined)
                return null;

            var lyricsUrl = hit.GetProperty("result").GetProperty("url").GetString();

            // Sử dụng Web Scraping để lấy lyrics từ trang Genius
            if (!string.IsNullOrEmpty(lyricsUrl))
            {
                return await ScrapeLyricsFromGeniusPageAsync(lyricsUrl);
            }

            return null;
        }

        private async Task<string?> ScrapeLyricsFromGeniusPageAsync(string lyricsUrl)
        {
            var response = await _httpClient.GetStringAsync(lyricsUrl);
            var document = new HtmlDocument();
            document.LoadHtml(response);

            var lyricsDiv = document.DocumentNode.SelectSingleNode("//div[@class='lyrics']") ??
                            document.DocumentNode.SelectSingleNode("//div[contains(@class, 'Lyrics__Container')]");

            return lyricsDiv?.InnerText?.Trim();
        }

        public async Task<(string addedAtString, DateTime addedAtTime)> AddDayOnly()
        {
            string addedAtString = DateTime.UtcNow.AddHours(7).ToString();
            string addedAtString2 = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            DateTime addedAtTime = DateTime.UtcNow.AddHours(7);
            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;
            DateTimeOffset addedAtOffset = DateTimeOffset.UtcNow.AddHours(7);

            string dateOnly = "2024-10-07";


            Test test = new()
            {
                DateTimeValue = addedAtTime,
                DateOnly = DateTime.Parse(dateOnly).Date.ToUniversalTime(),
                DateTimeString = addedAtString,
                DateTimeString2 = addedAtString2,
                DateTimeOffset = dateTimeOffset,
                DateTimeOffsetAddHours = addedAtOffset,
            };

            await _unitOfWork.GetCollection<Test>().InsertOneAsync(test);

            return (addedAtString, addedAtTime);
        }
    }
}
