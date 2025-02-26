using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using BusinessLogicLayer.ModelView.Service_Model_Views.AudioFeatures.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SetupLayer.Enum.Services.AudioFeature;

namespace BusinessLogicLayer.Implement.Services.Recommendation
{
    public class RecommendationBLL(IUnitOfWork unitOfWork, IMapper mapper) : IRecommendation
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrackResponseModel>> GetRecommendations(AudioFeaturesRequest audioFeaturesRequest, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1)
        {
            AudioFeatures matchedAudioFeatures = _mapper.Map<AudioFeatures>(audioFeaturesRequest);

            // Lấy danh sách Track có AudioFeatures trùng Mode để giảm số lượng cần so sánh
            IEnumerable<Track> similarTracks = await _unitOfWork.GetCollection<Track>()
                .Find(track => track.AudioFeatures.Mode == audioFeaturesRequest.Mode)
                .ToListAsync();

            // Tính toán độ tương đồng và sắp xếp trực tiếp trên danh sách truy vấn
            IEnumerable<Track> topSimilarTracks = similarTracks
                .Select(track => new
                {
                    Track = track,
                    Similarity = similarityScore(matchedAudioFeatures, track.AudioFeatures)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(k)
                .Select(x => x.Track)
                .ToList();

            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.StreamingUrl)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent
                .Match(track => topSimilarTracks.Select(t => t.Id).Contains(track.Id)) // Lọc theo các track đã chọn
                .Lookup<Track, Artist, ASTrack>
                (
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists
                )
                .Project<ASTrack>(projectionDefinition);

            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();

            // Mapping sang TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }


        public async Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1)
        {
            // Lấy AudioFeatures của track đầu vào
            Track matchedTrack = await _unitOfWork.GetCollection<Track>()
                .Find(t => t.Id == trackId)
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("Matched track not found");

            AudioFeatures matchedAudioFeatures = matchedTrack.AudioFeatures;

            // Truy vấn các track khác (không bao gồm track đầu vào)
            IEnumerable<Track> similarTracks = await _unitOfWork.GetCollection<Track>()
                .Find(t => t.Id != trackId)
                .ToListAsync();

            // Tính toán độ tương đồng và sắp xếp
            IEnumerable<Track> topSimilarTracks = similarTracks
                .Select(track => new
                {
                    Track = track,
                    Similarity = similarityScore(matchedAudioFeatures, track.AudioFeatures)
                })
                .OrderByDescending(x => x.Similarity)
                .Where(x => x.Similarity > 0.5)
                .Take(k)
                .Select(x => x.Track)
                .ToList();

            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.StreamingUrl)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent
                .Match(track => topSimilarTracks.Select(t => t.Id).Contains(track.Id)) // Lọc theo các track đã chọn
                .Lookup<Track, Artist, ASTrack>
                (
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists
                )
                .Project<ASTrack>(projectionDefinition);

            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();

            // Mapping các track tương tự thành TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }


        public async Task<IEnumerable<TrackResponseModel>> GetManyRecommendations(IEnumerable<string> trackIds, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1)
        {
            // Lấy danh sách AudioFeatures của các track đầu vào
            IEnumerable<AudioFeatures> matchedAudioFeaturesList = await _unitOfWork.GetCollection<Track>()
                .Find(track => trackIds.Contains(track.Id))
                .Project(track => track.AudioFeatures)
                .ToListAsync();

            if (!matchedAudioFeaturesList.Any())
            {
                throw new InvalidDataCustomException("No audio features found for the provided track IDs.");
            }

            // Lấy tất cả Track không thuộc danh sách track đầu vào
            IEnumerable<Track> allTracks = await _unitOfWork.GetCollection<Track>()
                .Find(track => !trackIds.Contains(track.Id))
                .ToListAsync();

            // Tính toán độ tương đồng cho tất cả tracks
            IEnumerable<string> similarityResults = matchedAudioFeaturesList
                .SelectMany(matchedAudioFeature =>
                    allTracks.Select(track => new
                    {
                        MatchedFeature = matchedAudioFeature,
                        Track = track,
                        Similarity = similarityScore(matchedAudioFeature, track.AudioFeatures)
                    }))
                .Where(x => x.Similarity > 0.5) // Lọc các bài hát có độ tương đồng lớn hơn 0.5
                .GroupBy(x => x.MatchedFeature) // Nhóm theo audio feature đầu vào
                .SelectMany(group => group
                    .OrderByDescending(x => x.Similarity) // Sắp xếp theo độ tương đồng
                    .Take(3)) // Lấy top 3 bài hát tương tự
                .Select(x => x.Track.Id)
                .Distinct() // Đảm bảo không có ID trùng lặp
                .ToList();

            #region Lấy danh sách Track tương ứng
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.StreamingUrl)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent
                .Match(track => similarityResults.Contains(track.Id)) // Match theo track Id
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project<ASTrack>(projectionDefinition);

            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();
            #endregion

            // Mapping sang TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }


        #region Sử dụng thuật toán Weighted Euclidean Distance
        public static double CalculateWeightedEulideanDisctance(AudioFeatures song1, AudioFeatures song2)
        {
            // Valence,         [0, 1]
            // Energy,          [0, 1]
            // Danceability,    [0, 1]
            // Tempo,           [20, 500] (BPM) 20 là tối thiểu, 500 là tối đa mà con người có thể phân biệt được
            // Acousticness, Instrumentalness, Speechiness [0, 1] - [0, 1] - [0, 1]
            // Mode             [0, 1]
            // Chuẩn hóa các đặc trưng theo range [0, 1]
            // Công thức chuẩn hóa: (x - min) / (max - min)
            // Trong đó: x là giá trị cần chuẩn hóa, min là giá trị nhỏ nhất trong tập hợp, max là giá trị lớn nhất trong tập hợp
            // Ví dụ: Chuẩn hóa giá trị 0.5 trong tập hợp [0, 1] => (0.5 - 0) / (1 - 0) = 0.5
            // Chuẩn hóa theo miền giá trị [0, 1] của các đặc trưng
            double keyStandardization1 = Standardize(song1.Key, (double)Key.C, (double)Key.B);
            double keyStandardization2 = Standardize(song2.Key, (double)Key.C, (double)Key.B);
            double tempoStandardization1 = Standardize(song1.Tempo, (double)Tempo.MinimumBPM, (double)Tempo.MaximumBPM);
            double tempoStandardization2 = Standardize(song2.Tempo, (double)Tempo.MinimumBPM, (double)Tempo.MaximumBPM);

            //double AISAvarageSong1 = (song1.Acousticness + song1.Instrumentalness + song1.Speechiness) / 3;
            //double AISAvarageSong2 = (song2.Acousticness + song2.Instrumentalness + song2.Speechiness) / 3;

            // Tập hợp các đặc trưng cần so sánh
            // Tập hợp X: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features1 = [
                //keyModeAvarageSong1,
                //song1.Key,
                keyStandardization1,
                song1.Valence, song1.Energy, song1.Danceability, tempoStandardization1,
                song1.Acousticness, song1.Instrumentalness, song1.Speechiness,
            ];

            // Tập hợp Y: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features2 = [
                //keyModeAvarageSong2,
                //song2.Key,
                keyStandardization2,
                song2.Valence, song2.Energy, song2.Danceability, tempoStandardization2,
                song2.Acousticness, song2.Instrumentalness, song2.Speechiness,
            ];

            // Đảm bảo cả hai tập hợp có cùng độ dài
            if (features1.Length != features2.Length)
            {
                throw new InvalidDataCustomException("Features length mismatch");
            }

            // Tính các trọng số cho các đặc trưng
            // Để cân bằng giữa các đặc trưng, ta đặt hệ số giảm dần từ 1 đến 0
            // Hệ số giảm dần alpha = 1, beta = 0.8, gamma = 0.6, medium = 0.5, delta = 0.4, epsilon = 0.2
            // Ta đặt W là trọng số
            // Thay vì tính tổng Sigma, ta sử dụng công thức tính tổng cấp số nhân
            // Điều này giúp tránh việc dùng vòng lặp
            // Công thức tính các trọng số Weights theo quy tắc cấp số nhân giảm dần với một hệ số
            // Đặt W(i) có trọng số cao nhất < 1 là k​
            // Các phép tính được ghi trong notebook
            double coefficientDescending = (double)CoefficientDescending.Beta / 100;
            double upperLimit = features1.Length;

            // Tính k
            double k = coefficientDescending == 1
                ? 1
                : (1 - Math.Pow(coefficientDescending, upperLimit)) / (1 - coefficientDescending);

            // Tính khoảng cách Euclidean giữa hai track
            double distance = Math.Sqrt(Enumerable
                                .Range(0, features1.Length)
                                .Sum(index => Math.Pow(
                                    (features1[index] * (Math.Pow(coefficientDescending, index) * k))
                                    - (features2[index] * (Math.Pow(coefficientDescending, index) * k)),
                                 2)));

            // Tính toán độ tương đồng giữa hai track
            double normalizedSimilarity = 1 / (1 + Math.Sqrt(distance));

            if (song1.Mode != song2.Mode)
            {
                normalizedSimilarity *= -1;
            }

            //Console.WriteLine("===========================");
            //Console.WriteLine($"{normalizedSimilarity}");
            //Console.WriteLine("===========================");

            return normalizedSimilarity;
        }
        #endregion

        #region Sử dụng thuật toán Cosine Similarity
        // https://en.wikipedia.org/wiki/Cosine_similarity
        // Để tính toán Cosine Similarity, cần chuẩn hóa các đặc trưng về cùng một range
        public static double CalculateCosineSimilarity(AudioFeatures song1, AudioFeatures song2)
        {
            double keyStandardization1 = Standardize(song1.Key, (double)Key.C, (double)Key.B);
            double keyStandardization2 = Standardize(song2.Key, (double)Key.C, (double)Key.B);
            double tempoStandardization1 = Standardize(song1.Tempo, (double)Tempo.MinimumBPM, (double)Tempo.MaximumBPM);
            double tempoStandardization2 = Standardize(song2.Tempo, (double)Tempo.MinimumBPM, (double)Tempo.MaximumBPM);

            // Tập hợp các đặc trưng cần so sánh
            // Tập hợp X: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features1 = [
                //keyModeAvarageSong1,
                //song1.Key,
                keyStandardization1,
                song1.Valence, song1.Energy, song1.Danceability, tempoStandardization1,
                song1.Acousticness, song1.Instrumentalness, song1.Speechiness,
            ];

            // Tập hợp Y: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features2 = [
                //keyModeAvarageSong2,
                //song2.Key,
                keyStandardization2,
                song2.Valence, song2.Energy, song2.Danceability, tempoStandardization2,
                song2.Acousticness, song2.Instrumentalness, song2.Speechiness,
            ];

            // Đảm bảo cả hai tập hợp có cùng độ dài
            if (features1.Length != features2.Length)
            {
                throw new InvalidDataCustomException("Features length mismatch");
            }

            double dotProduct = features1.Zip(features2, (a, b) => a * b).Sum();
            double magnitude1 = features1.Sum(a => a * a);
            double magnitude2 = features2.Sum(b => b * b);

            double cosineSimilarity = dotProduct / Math.Sqrt((magnitude1 * magnitude2));

            #region Dùng vòng lặp
            //double dotProduct = 0, magnitude1 = 0, magnitude2 = 0;

            //for (int i = 0; i < features1.Length; i++)
            //{
            //    dotProduct += features1[i] * features2[i];
            //    magnitude1 += features1[i] * features1[i];
            //    magnitude2 += features2[i] * features2[i];
            //}

            //if (magnitude1 == 0 || magnitude2 == 0) return 0;

            //double cosineSimilarity = dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
            #endregion

            if (song1.Mode != song2.Mode)
            {
                cosineSimilarity *= -1;
            }

            //Console.WriteLine("===========================");
            //Console.WriteLine($"{cosineSimilarity}");
            //Console.WriteLine("===========================");

            return cosineSimilarity;
        }
        #endregion

        private static double Standardize(double value, double min = 0, double max = 0)
        {
            bool isValidMinMax = min < max;
            bool isValidMinValue = value >= min;
            bool isValidMaxValue = value <= max;
            bool isValidValue = isValidMinMax && isValidMinValue && isValidMaxValue;

            if (!isValidValue)
            {
                throw new InvalidDataCustomException("Invalid value");
            }

            return (value - min) / (max - min);
        }
    }
}
