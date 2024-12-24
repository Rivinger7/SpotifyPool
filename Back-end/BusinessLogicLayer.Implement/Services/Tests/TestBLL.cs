using AutoMapper;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using System.Net.Http.Headers;
using System.Text.Json;
using HtmlAgilityPack;
using MongoDB.Driver;
using Spectrogram;
using Microsoft.AspNetCore.Http;

namespace BusinessLogicLayer.Implement.Services.Tests
{
    public class TestBLL(IUnitOfWork unitOfWork, IMapper mapper, HttpClient httpClient)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly HttpClient _httpClient = httpClient;

        //private (double[] audio, int sampleRate) ReadMono(string filename)
        //{
        //    using var reader = new NAudio.Wave.AudioFileReader(filename);
        //    var audio = new double[reader.Length / 2];
        //    reader.Read(audio, 0, audio.Length);
        //    return (audio, reader.WaveFormat.SampleRate);
        //}

        private static (double[] audio, int sampleRate) ReadMono(string filePath, double multiplier = 16_000)
        {
            using var afr = new NAudio.Wave.AudioFileReader(filePath);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
                audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
        }

        public async Task TestSpectrogram(IFormFile audioFile)
        {
            if (audioFile == null)
                return;

            // Đường dẫn thư mục bạn muốn sử dụng để lưu file (thư mục "Uploads" trong ổ C)
            string uploadDirectory = @"Z:\SpotifyPool Project\Images\Audio";

            // Kiểm tra xem thư mục có tồn tại không, nếu không thì tạo mới
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Đường dẫn lưu file tạm thời trên máy chủ
            string tempFilePath = Path.Combine(uploadDirectory, Guid.NewGuid() + Path.GetExtension(audioFile.FileName));

            // Lưu file vào đường dẫn
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Đường dẫn lưu file Spectrogram
            const string FILE_SAVE_PATH = @"Z:\SpotifyPool Project\Images\Results";
            string fileName = Path.GetFileNameWithoutExtension(audioFile.FileName);

            try
            {
                // Thực hiện các xử lý sau khi đã có đường dẫn file
                (double[] audio, int sampleRate) = ReadMono(tempFilePath);

                int fftSize = 16384;
                int targetWidthPx = 3000;
                int stepSize = audio.Length / targetWidthPx;

                var sg = new SpectrogramGenerator(sampleRate, fftSize, stepSize, maxFreq: 2200);
                sg.Add(audio);

                // Đường dẫn lưu kết quả
                string resultFilePath = Path.Combine(FILE_SAVE_PATH, fileName + ".png");
                sg.SaveImage(resultFilePath, intensity: 5, dB: true);

                // Log hoặc xử lý kết quả lưu
                //Console.WriteLine($"Spectrogram saved at {resultFilePath}");
            }
            finally
            {
                // Xóa file tạm sau khi xử lý xong để giải phóng bộ nhớ
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        public async Task TestTopTrack(string trackId){
            // Lấy thông tin user từ Context
            string? userId = "6736c563216626b7bf5f1441";

			string? topItemId = await _unitOfWork.GetCollection<TopTrack>()
                                                 .Find(topItem => topItem.UserId == userId) //&& topItem.TrackInfo.Any(track => track.TrackId == trackId))
	                                             .Project(topItem => topItem.Id)
	                                             .FirstOrDefaultAsync();

			if (topItemId is null)
            {

				// create new
				TopTrack newTopItem = new ()
				{
					UserId = userId,
					TrackInfo =
					[
						new TopTrackInfo
						{
							TrackId = trackId,
							StreamCount = 1
						}
					],
				};
				await _unitOfWork.GetCollection<TopTrack>().InsertOneAsync(newTopItem);
				return;
			}


            TopTrackInfo? trackInfo = await _unitOfWork.GetCollection<TopTrack>()
                                             .Find(topItem => topItem.Id == topItemId && topItem.TrackInfo.Any(track => track.TrackId == trackId))
                                             .Project(topItem => topItem.TrackInfo.FirstOrDefault(track => track.TrackId == trackId))
                                             .FirstOrDefaultAsync();

            if (trackInfo is null){
                // add new track
                FilterDefinition<TopTrack> addTrackInfofilter = Builders<TopTrack>.Filter.Eq(topItem => topItem.Id, topItemId);

                UpdateDefinition<TopTrack> addTrackInfoUpdateDefinition = Builders<TopTrack>.Update.Push(topItem => topItem.TrackInfo, new TopTrackInfo
                {
                    TrackId = trackId,
                    StreamCount = 1
                });
                UpdateOptions addTrackInfoUpdateOptions = new() { IsUpsert = false };

                await _unitOfWork.GetCollection<TopTrack>().UpdateOneAsync(addTrackInfofilter, addTrackInfoUpdateDefinition, addTrackInfoUpdateOptions);
                return;
            }

            var filter = Builders<TopTrack>.Filter.And(
                Builders<TopTrack>.Filter.Eq(topItem => topItem.Id, topItemId),
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
