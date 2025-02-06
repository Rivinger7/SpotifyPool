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

            // Lấy danh sách AudioFeatures trùng với Mode
            // Truy vấn mode để giảm số lượng audioFeatures cần so sánh
            IEnumerable<AudioFeatures> similarAudioFeatures = await _unitOfWork.GetCollection<AudioFeatures>()
                .Find(f => f.Mode == audioFeaturesRequest.Mode)
                .ToListAsync();

            // Tính toán độ tương đồng và sắp xếp trực tiếp trên danh sách truy vấn
            // Trả về AudioFeaturesIds của k track tương tự nhất
            IEnumerable<string> topSimilarFeatures = similarAudioFeatures
                .Select(audioFeatures => new
                {
                    Feature = audioFeatures,
                    //Similarity = CalculateSimilarity(matchedAudioFeatures, audioFeatures)
                    Similarity = similarityScore(matchedAudioFeatures, audioFeatures)
                })
                .OrderByDescending(x => x.Similarity)
                .Where(x => x.Similarity > 0.5)
                .Take(k)
                .Select(x => x.Feature.Id)
                .ToList();

            #region Lấy danh sách Track tương ứng với các AudioFeatures đã lọc
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent
                .Match(track => topSimilarFeatures.Contains(track.AudioFeaturesId)) // Match the track by audioFeaturesId
                .Lookup<Track, Artist, ASTrack>
                (_unitOfWork.GetCollection<Artist>(), // The foreign collection
                track => track.ArtistIds, // The field in Track that are joining on
                artist => artist.Id, // The field in Artist that are matching against
                result => result.Artists) // The field in ASTrack to hold the matched artists
                .Project<ASTrack>(projectionDefinition);

            // To list
            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();
            #endregion

            // Mapping các track tương tự thành TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1)
        {
            // Empty Pipeline
            IAggregateFluent<Track> trackAggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lookup
            AudioFeatures matchedAudioFeatures = await trackAggregateFluent
                .Match(track => track.Id == trackId)
                .Lookup<Track, AudioFeatures, ASTrack>(
                    _unitOfWork.GetCollection<AudioFeatures>(),
                    track => track.AudioFeaturesId,
                    audioFeature => audioFeature.Id,
                    result => result.AudioFeatures)
                // Phân rã mảng hoặc danh sách audioFeatures thành một document duy nhất
                .Unwind(result => result.AudioFeatures, new AggregateUnwindOptions<ASTrack>
                {
                    // Danh sách trường cần giữ nguyên giá trị null hoặc rỗng
                    PreserveNullAndEmptyArrays = true
                })
                .Project(result => result.AudioFeatures)
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("Matched audio features not found");

            // Truy vấn các đặc trưng âm thanh tương tự với track đầu vào
            IEnumerable<AudioFeatures> similarFeatures = await _unitOfWork.GetCollection<AudioFeatures>()
                .Find(f => f.Id != matchedAudioFeatures.Id)
                .ToListAsync();

            // Tính toán độ tương đồng và sắp xếp trực tiếp trên danh sách truy vấn
            // Trả về AudioFeaturesIds của k track tương tự nhất
            IEnumerable<string> topSimilarFeatures = similarFeatures
                .Select(audioFeatures => new
                {
                    Feature = audioFeatures,
                    //Similarity = CalculateSimilarity(matchedAudioFeatures, audioFeatures)
                    Similarity = similarityScore(matchedAudioFeatures, audioFeatures)
                })
                .OrderByDescending(x => x.Similarity)
                .Where(x => x.Similarity > 0.5)
                .Take(k)
                .Select(x => x.Feature.Id)
                .ToList();

            #region Lấy danh sách Track tương ứng với các AudioFeatures đã lọc
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent
                .Match(track => topSimilarFeatures.Contains(track.AudioFeaturesId)) // Match the track by audioFeaturesId
                .Lookup<Track, Artist, ASTrack>
                (_unitOfWork.GetCollection<Artist>(), // The foreign collection
                track => track.ArtistIds, // The field in Track that are joining on
                artist => artist.Id, // The field in Artist that are matching against
                result => result.Artists) // The field in ASTrack to hold the matched artists
                .Project<ASTrack>(projectionDefinition);

            // To list
            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();
            #endregion

            // Mapping các track tương tự thành TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetManyRecommendations(IEnumerable<string> trackIds, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 1)
        {
            // Stage
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy danh sách AudioFeatures của các track đầu vào
            IEnumerable<AudioFeatures?> matchedAudioFeaturesList = await aggregateFluent
                .Match(track => trackIds.Contains(track.Id))
                .Lookup<Track, AudioFeatures, ASTrack>(
                    _unitOfWork.GetCollection<AudioFeatures>(),
                    track => track.AudioFeaturesId,
                    audioFeature => audioFeature.Id,
                    result => result.AudioFeatures)
                .Unwind(result => result.AudioFeatures, new AggregateUnwindOptions<ASTrack>
                {
                    PreserveNullAndEmptyArrays = true
                })
                .Project(result => result.AudioFeatures)
                .ToListAsync();

            if (!matchedAudioFeaturesList.Any())
            {
                throw new InvalidDataCustomException("No audio features found for the provided track IDs.");
            }

            // Truy vấn tất cả AudioFeatures không trùng với các track đầu vào
            IEnumerable<AudioFeatures> allAudioFeatures = await _unitOfWork.GetCollection<AudioFeatures>()
                .Find(f => !matchedAudioFeaturesList.Select(m => m.Id).Contains(f.Id))
                .ToListAsync();

            IEnumerable<string> similarityResults = matchedAudioFeaturesList
                .SelectMany(matchedAudioFeature =>
                    allAudioFeatures.Select(audioFeature => new
                    {
                        MatchedFeature = matchedAudioFeature,
                        Feature = audioFeature,
                        Similarity = similarityScore(matchedAudioFeature ?? throw new InvalidDataCustomException("No audio features found for the provided track IDs."), audioFeature)
                    }))
                .Where(x => x.Similarity > 0.5) // Lọc các bài hát có độ tương đồng lớn hơn 0.5
                .GroupBy(x => x.MatchedFeature?.Id) // Nhóm theo bài hát gốc
                .SelectMany(group => group
                    .OrderByDescending(x => x.Similarity) // Sắp xếp theo độ tương đồng
                    .Take(3)) // Lấy top 3 bài hát tương tự
                .Select(x => x.Feature.Id)
                .Distinct() // Đảm bảo không có ID trùng lặp
                .ToList();

            #region Lấy danh sách Track tương ứng với các AudioFeatures đã lọc
            // Projection
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.Lyrics)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent2 = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thêm thông tin về artist
            IAggregateFluent<ASTrack> trackPipelines = aggregateFluent2
                .Match(track => similarityResults.Contains(track.AudioFeaturesId)) // Match the track by audioFeaturesId
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project<ASTrack>(projectionDefinition);

            // To list
            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();
            #endregion

            // Mapping các track tương tự thành TrackResponseModel
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
