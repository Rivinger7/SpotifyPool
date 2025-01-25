using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using HtmlAgilityPack;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BusinessLogicLayer.Implement.Services.Tests
{
    public class TestBLL(IUnitOfWork unitOfWork, HttpClient httpClient)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly HttpClient _httpClient = httpClient;

        public async Task TestOpenApi()
        {

        }

        public int TestSearchPredict(string text)
        {
            // Đường dẫn mô hình ONNX và tokenizer
            string currentDirectory = Directory.GetCurrentDirectory();
            string modelPath = Path.Combine(currentDirectory, "4. Application", "nlp_vietnamese_model.onnx");
            string vocabPath = Path.Combine(currentDirectory, "4. Application", "vocab.txt");
            // Tạo tokenizer từ vocab
            var tokenizer = BertTokenizer.Create(vocabPath);

            // Nhập câu văn bản từ người dùng

            // Tokenize câu văn bản
            var tokens = tokenizer.EncodeToIds("Cuộc sống thật chán nản, chẳng có chút động lực nào.");
            var inputIds = tokens.ToList();

            // Tạo attention mask (1 cho token thật, 0 cho padding)
            var attentionMask = new List<long>(inputIds.Count);
            foreach (var _ in inputIds) attentionMask.Add(1);

            // Padding/truncation
            const int maxSequenceLength = 128;
            if (inputIds.Count > maxSequenceLength)
            {
                inputIds = inputIds.Take(maxSequenceLength).ToList();
                attentionMask = attentionMask.Take(maxSequenceLength).ToList();
            }
            else
            {
                while (inputIds.Count < maxSequenceLength)
                {
                    inputIds.Add(0); // Padding ID
                    attentionMask.Add(0); // Padding Mask
                }
            }

            // Chuyển dữ liệu thành DenseTensor
            var inputIdsTensor = new DenseTensor<long>(new[] { 1, maxSequenceLength });
            var attentionMaskTensor = new DenseTensor<long>(new[] { 1, maxSequenceLength });
            for (int i = 0; i < maxSequenceLength; i++)
            {
                inputIdsTensor[0, i] = inputIds[i];
                attentionMaskTensor[0, i] = attentionMask[i];
            }

            // Load ONNX Runtime và chạy dự đoán
            using var session = new InferenceSession(modelPath);
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
            };
            using var results = session.Run(inputs);

            // Lấy kết quả output
            var output = results.First().AsEnumerable<float>().ToArray();

            // Xác định nhãn cảm xúc (Emotion Level)
            int predictedEmotion = Array.IndexOf(output, output.Max()) + 1; // Nhãn từ 1-5
            Console.WriteLine($"Kết quả: Mức độ cảm xúc là {predictedEmotion}");

            return predictedEmotion;
        }

        public static async Task<IEnumerable<string>> TestImgx()
        {
            string imagePath = "https://i.scdn.co/image/ab67616d0000b2736f7be334503038d1a40c873a";

            IEnumerable<Color> colors = await ExtractPalette(imagePath, 9);

            Console.WriteLine("Extracted Palette:");
            foreach (Color color in colors)
            {
                Console.WriteLine($"RGB({color.R}, {color.G}, {color.B})");
            }

            colors = colors.OrderBy(c => c.GetBrightness());
            IEnumerable<string> hexColors = colors.Select(color => $"#{color.R:X2}{color.G:X2}{color.B:X2}").ToList();

            return hexColors;
        }

        private static List<Color> GetPaletteFromBitmap(Bitmap bitmap, int colorCount)
        {
            // 1. Giảm kích thước ảnh (resizing)
            const int targetWidth = 200;
            const int targetHeight = 200;

            Bitmap resizedBitmap = new(bitmap, new Size(targetWidth, targetHeight));

            // 2. Lấy dữ liệu pixel
            var pixels = new List<Color>();
            for (int y = 0; y < resizedBitmap.Height; y++)
            {
                for (int x = 0; x < resizedBitmap.Width; x++)
                {
                    var pixelColor = resizedBitmap.GetPixel(x, y);
                    if (pixelColor.A > 0) // Bỏ qua các pixel trong suốt
                    {
                        pixels.Add(pixelColor);
                    }
                }
            }

            // 3. Áp dụng thuật toán K-Means Clustering để gom nhóm màu
            return ApplyKMeansClustering(pixels, colorCount);
        }

        // Hàm K-Means Clustering để nhóm màu sắc
        private static List<Color> ApplyKMeansClustering(List<Color> pixels, int k)
        {
            // Chuyển đổi pixel thành vector (RGB)
            var pixelVectors = pixels.Select(c => new[] { c.R, c.G, c.B }).ToArray();

            // Khởi tạo các centroid ngẫu nhiên
            Random random = new();
            var centroids = Enumerable.Range(0, k)
                .Select(_ => new int[]
                {
                    pixelVectors[random.Next(pixelVectors.Length)][0],
                    pixelVectors[random.Next(pixelVectors.Length)][1],
                    pixelVectors[random.Next(pixelVectors.Length)][2]
                })
                .ToArray();

            bool centroidsChanged = true;

            while (centroidsChanged)
            {
                // Gom nhóm các pixel dựa trên centroid gần nhất
                var clusters = pixelVectors
                    .Select(pixel => pixel.Select(b => (int)b).ToArray()) // Chuyển từ byte[] sang int[]
                    .GroupBy(p => FindClosestCentroid(p, centroids))
                    .ToArray();

                // Cập nhật centroid mới dựa trên trung bình của các cluster
                var newCentroids = clusters
                    .Select(cluster => cluster
                        .Aggregate(new double[3], (acc, p) =>
                        {
                            acc[0] += p[0];
                            acc[1] += p[1];
                            acc[2] += p[2];
                            return acc;
                        }, acc =>
                        {
                            acc[0] /= cluster.Count();
                            acc[1] /= cluster.Count();
                            acc[2] /= cluster.Count();
                            return acc.Select(v => (int)v).ToArray();
                        })
                    ).ToArray();

                // Kiểm tra nếu centroid không thay đổi
                centroidsChanged = !centroids.SequenceEqual(newCentroids, new IntArrayComparer());
                centroids = newCentroids;
            }

            // Trả về danh sách màu sắc từ centroid
            return centroids
                .Select(c => Color.FromArgb(c[0], c[1], c[2]))
                .ToList();
        }


        // Hàm tìm centroid gần nhất
        private static int FindClosestCentroid(int[] pixel, int[][] centroids)
        {
            int minIndex = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < centroids.Length; i++)
            {
                double distance = Math.Sqrt(
                    Math.Pow(pixel[0] - centroids[i][0], 2) +
                    Math.Pow(pixel[1] - centroids[i][1], 2) +
                    Math.Pow(pixel[2] - centroids[i][2], 2)
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        // So sánh mảng int[] (cho centroid)
        private class IntArrayComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y)
            {
                if (x.Length != y.Length) return false;
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }
                return true;
            }

            public int GetHashCode(int[] obj)
            {
                return obj.Sum();
            }
        }


        public static async Task<List<Color>> ExtractPalette(string imagePath, int colorCount)
        {
            Bitmap bitmap;

            // Kiểm tra và tải hình ảnh
            if (Uri.IsWellFormedUriString(imagePath, UriKind.Absolute))
            {
                using HttpClient client = new();
                using Stream stream = await client.GetStreamAsync(imagePath);
                bitmap = new Bitmap(stream);
            }
            else if (File.Exists(imagePath))
            {
                bitmap = new Bitmap(imagePath);
            }
            else
            {
                throw new FileNotFoundException($"The image path '{imagePath}' is invalid or not accessible.");
            }

            // Xử lý palette
            return GetPaletteFromBitmap(bitmap, colorCount);
        }

        public static List<Color> QuantizeColors(List<Color> pixels, int colorCount)
        {
            // Sử dụng linq để nhóm các màu gần nhau
            var groups = pixels
                .GroupBy(p => new { R = p.R / 32, G = p.G / 32, B = p.B / 32 })
                .OrderByDescending(g => g.Count())
                .Take(colorCount);

            // Trả về các màu trung bình của mỗi nhóm
            return groups.Select(g =>
            {
                int avgR = (int)g.Average(p => p.R);
                int avgG = (int)g.Average(p => p.G);
                int avgB = (int)g.Average(p => p.B);
                return Color.FromArgb(avgR, avgG, avgB);
            }).ToList();
        }

        public string[] GetFilePath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "SpectrogramTempData", $"{Guid.NewGuid}.txt");

            string[] paths = [currentDirectory, path];

            File.WriteAllText(path, "Hello World!");

            return paths;
        }

        public async Task TestTopTrack(string trackId)
        {
            // Lấy thông tin user từ Context
            string? userId = "6736c563216626b7bf5f1441";

            string? topItemId = await _unitOfWork.GetCollection<TopTrack>()
                                                 .Find(topItem => topItem.UserId == userId) //&& topItem.TrackInfo.Any(track => track.TrackId == trackId))
                                                 .Project(topItem => topItem.Id)
                                                 .FirstOrDefaultAsync();

            if (topItemId is null)
            {

                // create new
                TopTrack newTopItem = new()
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

            if (trackInfo is null)
            {
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
