﻿using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Recommendation;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;

namespace BusinessLogicLayer.Implement.Services.Recommendation
{
    public class RecommendationBLL(IUnitOfWork unitOfWork, IMapper mapper) : IRecommendation
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, int k = 5)
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
                .FirstOrDefaultAsync();

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
                    Similarity = CalculateSimilarity(matchedAudioFeatures, audioFeatures)
                })
                .OrderByDescending(x => x.Similarity)
                //.Where(x => x.Similarity > 0.5)
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
                .Project(projectionDefinition)
                .As<ASTrack>();

            // To list
            IEnumerable<ASTrack> recommendedTracks = await trackPipelines.ToListAsync();
            #endregion

            // Mapping các track tương tự thành TrackResponseModel
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(recommendedTracks);

            return responseModels;
        }

        // Tính toán độ tương đồng giữa hai track
        // Sử dụng thuật toán Euclidean Distance
        // https://en.wikipedia.org/wiki/Euclidean_distance
        private static double CalculateSimilarity(AudioFeatures song1, AudioFeatures song2)
        {
            // Tập hợp các đặc trưng cần so sánh
            // Tập hợp X: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features1 = [
                song1.Acousticness, song1.Danceability, song1.Energy,
                song1.Instrumentalness, song1.Liveness, song1.Speechiness,
                song1.Tempo, song1.Valence
            ];

            // Tập hợp Y: [Acousticness, Danceability, Energy, Instrumentalness, Liveness, Speechiness, Tempo, Valence]
            double[] features2 = [
                song2.Acousticness, song2.Danceability, song2.Energy,
                song2.Instrumentalness, song2.Liveness, song2.Speechiness,
                song2.Tempo, song2.Valence
            ];

            // Đảm bảo cả hai tập hợp có cùng độ dài
            if (features1.Length != features2.Length)
            {
                throw new InvalidDataCustomException("Features length mismatch");
            }

            // Chuẩn hóa các đặc trưng theo range [0, 1]
            // Công thức chuẩn hóa: (x - min) / (max - min)
            // Trong đó: x là giá trị cần chuẩn hóa, min là giá trị nhỏ nhất trong tập hợp, max là giá trị lớn nhất trong tập hợp
            // Ví dụ: Chuẩn hóa giá trị 0.5 trong tập hợp [0, 1] => (0.5 - 0) / (1 - 0) = 0.5
            // Tạm thời chưa cần vì thuật toán KNN không cần chuẩn hóa dữ liệu

            // Khoảng cách giữa 2 đặc trưng
            double distanceMini = 0;

            // Tính tổng bình phương khoảng cách giữa các đặc trưng
            for (int index = 0; index < features1.Length; ++index)
            {
                distanceMini += Math.Pow(features1[index] - features2[index], 2);
            }

            // Tính khoảng cách Euclidean giữa hai track
            double distance = Math.Sqrt(distanceMini);

            // Tính toán độ tương đồng giữa hai track
            double similarity = 1 / (1 + Math.Sqrt(distance));

            //Console.WriteLine("===========================");
            //Console.WriteLine($"{similarity}");
            //Console.WriteLine("===========================");

            return similarity;
        }

        // Tính toán độ tương đồng giữa hai track
        // Sử dụng thuật toán cosine similarity
        // https://en.wikipedia.org/wiki/Cosine_similarity
        //private static double CalculateSimilarity(AudioFeatures song1, AudioFeatures song2)
        //{
        //    double[] features1 = [
        //        song1.Acousticness, song1.Danceability, song1.Energy,
        //        song1.Instrumentalness, song1.Liveness, song1.Speechiness,
        //        song1.Tempo, song1.Valence
        //    ];

        //    double[] features2 = [
        //        song2.Acousticness, song2.Danceability, song2.Energy,
        //        song2.Instrumentalness, song2.Liveness, song2.Speechiness,
        //        song2.Tempo, song2.Valence
        //    ];

        //    double dotProduct = 0, magnitude1 = 0, magnitude2 = 0;

        //    for (int i = 0; i < features1.Length; i++)
        //    {
        //        dotProduct += features1[i] * features2[i];
        //        magnitude1 += features1[i] * features1[i];
        //        magnitude2 += features2[i] * features2[i];
        //    }

        //    if (magnitude1 == 0 || magnitude2 == 0) return 0;

        //    double cosineSimilarity = dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));

        //    Console.WriteLine("===========================");
        //    Console.WriteLine($"{cosineSimilarity}");
        //    Console.WriteLine("===========================");

        //    return cosineSimilarity;
        //}
    }
}
