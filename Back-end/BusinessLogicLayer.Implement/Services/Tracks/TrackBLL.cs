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
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync()
        {
            // Lấy tất cả các track với artist
            IEnumerable<ASTrack> tracks = await _unitOfWork.GetRepository<ASTrack>().GetAllTracksWithArtistAsync();

            // Map the aggregate result to TrackResponseModel
            IEnumerable<TrackResponseModel> responseModel = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return responseModel;
        }

        public async Task<TrackResponseModel> GetTrackAsync(string id)
        {
            // Lấy track với artist
            ASTrack track = await _unitOfWork.GetRepository<ASTrack>().GetTrackWithArtistAsync(id);

            // Map the aggregate result to TrackResponseModel
            TrackResponseModel responseModel = _mapper.Map<TrackResponseModel>(track);

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

            // Chỉ lấy những field cần thiết  
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Tạo bộ lọc cho ASTrack riêng biệt sau khi Lookup  
            FilterDefinition<ASTrack> artistFilter = Builders<ASTrack>.Filter.Or(
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Description, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.ElemMatch(track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase))
            );

            // Lookup from Artist collection to Track collection  
            IAggregateFluent<ASTrack> trackPipelines = pipeline.Lookup<Track, Artist, ASTrack> // Stage 1  
                (_unitOfWork.GetCollection<Artist>(),
                track => track.ArtistIds,
                artist => artist.Id,
                result => result.Artists)
                .Match(artistFilter)  // Tìm kiếm theo Artist hoặc TrackName // Stage 2  
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
