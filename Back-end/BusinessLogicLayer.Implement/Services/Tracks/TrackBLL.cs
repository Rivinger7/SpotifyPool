using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit)
        {
            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Skip((offset - 1) * limit)
                .Limit(limit)
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(trackWithArtistProjection)
                .ToListAsync();

            return tracksResponseModel;
        }



        public async Task<TrackResponseModel> GetTrackAsync(string id)
        {
            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    // Lý do không dùng được vì Expression không hỗ trợ các hàm extension
                    // DurationFormated = Util.FormatTimeFromMilliseconds(track.Duration),
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            TrackResponseModel trackResponseModel = await aggregateFluent
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(trackWithArtistProjection)
                .FirstOrDefaultAsync();

            return trackResponseModel;
        }

        public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        {
            // Xử lý các ký tự đặc biệt
            string searchTermEscaped = Util.EscapeSpecialCharacters(searchTerm);

            // Empty Pipeline
            IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Tạo bộ lọc cho ASTrack riêng biệt sau khi Lookup  
            FilterDefinition<ASTrack> artistFilter = Builders<ASTrack>.Filter.Or(
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Description, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.ElemMatch(track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase))
            );

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Match(artistFilter)
                .Project(trackWithArtistProjection)
                .ToListAsync();

            return tracksResponseModel;
        }

        public async Task<TopTrackResponseModel?> GetTopTracksAsync()
        {
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                                ?? throw new DataNotFoundCustomException("Your session is limit. Please login again.");


            //join TopTrack với Track, aggregate lại dạng list ASTopTrack để tí sửa trong list này, ko để IEnumerable
            List<ASTopTrack> topTracksWithTracks = await _unitOfWork.GetRepository<TopTrack>().Collection
                .Aggregate()
                .Match(topTrack => topTrack.UserId == userId)
                .Lookup<TopTrack, Track, ASTopTrack>(
                    foreignCollection: _unitOfWork.GetRepository<Track>().Collection,
                    localField: topTrack => topTrack.TrackInfo.Select(info => info.TrackId),
                    foreignField: track => track.Id,
                    @as: result => result.Tracks
                )
            .ToListAsync();



            // gộp thông tin từ list trên vào ASTopTrack mới
            TopTrackResponseModel? enrichedResult = topTracksWithTracks
            .Select(topTrack => new TopTrackResponseModel
            {
                TopTrackId = topTrack.TopTrackId,
                UserId = topTrack.UserId,
                TrackInfo = topTrack.TrackInfo.Select(info => new TracksInfoResponse
                {
                    //lấy mấy cái cần thiết liên quan đến Track rồi gán vào TrackInfo
                    TrackId = info.TrackId,
                    StreamCount = info.StreamCount,
                    FirstAccessTime = info.FirstAccessTime,
                    Track = _mapper.Map<TrackInTopTrackResponseModel>(topTrack.Tracks.FirstOrDefault(t => t.Id == info.TrackId)),
                    Artists = topTrack.Tracks.Where(t => t.Id == info.TrackId) //lấy track từ topTrack.Tracks có Id trùng với info.TrackId
                                                                               //lấy artist từ các track trong TrackInfo và duy nhất
                                    .SelectMany(track => track.ArtistIds)
                                    .Distinct()
                                    // lấy artist có artistId tương ứng đã select ở bên trên
                                    .Select(artistId => _unitOfWork.GetRepository<Artist>().Collection
                                        .Find(artist => artist.Id == artistId)
                                        .Project(artist => artist.Name)
                                        .FirstOrDefault()
                                    )
                                    .ToList()
                }).OrderByDescending(info => info.StreamCount).Skip((1 - 1) * 50).Take(50).ToList()
            }).FirstOrDefault();

            return enrichedResult;
        }
    }
}
