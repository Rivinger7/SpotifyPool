using AutoMapper;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    // CLASS NÀY VẪN ĐANG TEST
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync()
        {
            // Empty Pipeline
            IAggregateFluent<Track> pipeLine = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lookup
            IAggregateFluent<ASTrack> trackPipelines = pipeLine.Lookup<Track, Artist, ASTrack>
                (_unitOfWork.GetCollection<Artist>(), // The foreign collection
                track => track.ArtistIds, // The field in Track that are joining on
                artist => artist.SpotifyId, // The field in Artist that are matching against
                result => result.Artists) // The field in ASTrack to hold the matched artists
                .Project(Builders<ASTrack>.Projection  // Project
                .Include(ast => ast.Name) // Get only necessary fields
                .Include(ast => ast.Description)
                .Include(ast => ast.PreviewURL)
                .Include(ast => ast.Duration)
                .Include(ast => ast.Images)
                .Include(ast => ast.Artists)
                ).As<ASTrack>();

            // Pipeline to list
            IEnumerable<ASTrack> artistTracks = await trackPipelines.ToListAsync();

            // Map the aggregate result to TrackResponseModel
            IEnumerable<TrackResponseModel> responseModel = _mapper.Map<IEnumerable<TrackResponseModel>>(artistTracks);

            return responseModel;
        }

        public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        {
            // Nếu không có searchTerm thì trả về mảng rỗng
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return [];
            }

            // Xử lý các ký tự đặc biệt
            string searchTermEscaped = Util.EscapeSpecialCharacters(searchTerm);

            // Empty Pipeline
            IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            // Need to index before using this one
            FilterDefinition<Track> trackFilter = Builders<Track>.Filter.Or(
                Builders<Track>.Filter.Regex(track => track.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<Track>.Filter.Regex(track => track.Description, new BsonRegularExpression(searchTermEscaped, "i"))
            );

            //// Tạo bộ lọc cho Artist riêng biệt sau khi Lookup
            //FilterDefinition<ASTrack> artistFilter = Builders<ASTrack>.Filter.ElemMatch(
            //    track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase)  // Tìm kiếm theo Artist.Name
            //);

            // Chỉ lấy những field cần thiết
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Lookup from Artist collection to Track collection
            IAggregateFluent<ASTrack> trackPipelines = pipeline
                .Match(trackFilter) // Stage 1
                .Lookup<Track, Artist, ASTrack> // Stage 2
                (_unitOfWork.GetCollection<Artist>(),
                track => track.ArtistIds,
                artist => artist.SpotifyId,
                result => result.Artists)
                //.Match(artistFilter) // Lọc thêm theo tên ca sĩ sau khi Lookup // Stage 3
                .Project(projectionDefinition)
                .As<ASTrack>();

            // To list
            IEnumerable<Track> tracks = await trackPipelines.ToListAsync();

            // Mapping to response model
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return responseModels;
        }

    }
}
