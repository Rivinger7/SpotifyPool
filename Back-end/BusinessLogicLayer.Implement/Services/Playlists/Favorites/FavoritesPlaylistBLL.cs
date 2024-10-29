using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Favorites;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SetupLayer.Enum.Services.Playlist;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Playlists.Favorites
{
    public class FavoritesPlaylistBLL(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IMapper mapper) : IFavoritesPlaylist
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper _mapper = mapper;

        public async Task AddToPlaylistAsync(string trackID)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(playlist => playlist.UserID == userID).FirstOrDefaultAsync();

            if (playlist is null)
            {
                playlist = new()
                {
                    Name = PlaylistName.FavoriteSong,
                    Description = string.Empty,
                    UserID = userID,
                    CreatedTime = Util.GetUtcPlus7Time(),
                    TrackIds = [trackID],
                    Images =
                    [
                        new() {
                            URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-640_xnff8r.png",
                            Height = 640,
                            Width = 640,
                        },
                        new() {
                            URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-300_vitqvn.png",
                            Height = 300,
                            Width = 300,
                        },
                        new() {
                            URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-64_izigfw.png",
                            Height = 64,
                            Width = 64,
                        }
                    ]
                };

                await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);
                return;
            }

            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Contains(trackID))
            {
                throw new DataExistCustomException("The song has been added to your Favorite Song");
            }

            playlist.TrackIds.Add(trackID);
            // Cập nhật playlist với danh sách TrackIds mới
            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(p => p.TrackIds, playlist.TrackIds);
            UpdateResult updateResult = await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(p => p.Id == playlist.Id, updateDefinition);

            if(updateResult.ModifiedCount < 1)
            {
                throw new CustomException("Add to Favorite Songs", 44, "Can't add to your favorite songs");
            }
        }

        public async Task<FavoritesSongsResponseModel> GetPlaylistAsync()
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Chỉ lấy những fields cần thiết từ Playlist
            ProjectionDefinition<Playlist> playlistProjection = Builders<Playlist>.Projection
                .Exclude(playlist => playlist.UserID);

            // Lấy thông tin Playlist
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(playlist => playlist.UserID == userID)
                .Project(playlistProjection)
                .As<Playlist>()
                .FirstOrDefaultAsync()
                ?? throw new DataNotFoundCustomException($"Not found any playlist with User {userID}");

            // Chỉ lấy những thông tin cần thiết từ ASTrack : Track
            ProjectionDefinition<ASTrack> astrackProjection = Builders<ASTrack>.Projection  // Project  
                .Include(ast => ast.Name)
                .Include(ast => ast.Description)
                .Include(ast => ast.PreviewURL)
                .Include(ast => ast.Duration)
                .Include(ast => ast.Images)
                .Include(ast => ast.Artists);

            // Empty Pipeline
            IAggregateFluent<Track> trackPipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lookup  
            IEnumerable<ASTrack> tracks = await trackPipeline
                .Match(t => playlist.TrackIds.Contains(t.Id))
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    t => t.ArtistIds,
                    a => a.SpotifyId,
                    t => t.Artists
                ).Project(astrackProjection)
                .As<ASTrack>().ToListAsync();

            // Mapping the Playlist to FavoritesSongsResponseModel
            FavoritesSongsResponseModel playlistResponseModel = _mapper.Map<FavoritesSongsResponseModel>(playlist);

            // Mapping tracks with artists to TrackResponseModel
            playlistResponseModel.Tracks = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return playlistResponseModel;
        }

        public async Task RemoveFromPlaylistAsync(string trackID)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Chỉ lấy những fields cần thiết từ Playlist
            ProjectionDefinition<Playlist> playlistProjection = Builders<Playlist>.Projection
                .Include(playlist => playlist.Id)
                .Include(playlist => playlist.TrackIds);

            // Lấy thông tin Playlist
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(playlist => playlist.UserID == userID)
                .Project(playlistProjection).As<Playlist>()
                .FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException($"Not found any playlist with User {userID}");

            // Chỉ xóa trackID nếu tồn tại để tránh trùng lặp
            if (!playlist.TrackIds.Contains(trackID))
            {
                throw new DataExistCustomException("The song has not been added to your Favorite Song");
            }

            // Xóa trackID khỏi danh sách TrackIds
            playlist.TrackIds.Remove(trackID);

            // Nếu TrackIds rỗng thì xóa luôn playlist đó
            if (playlist.TrackIds.Count == 0)
            {
                await _unitOfWork.GetCollection<Playlist>().DeleteOneAsync(p => p.Id == playlist.Id);
                return;
            }

            // Cập nhật playlist với danh sách TrackIds mới
            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(p => p.TrackIds, playlist.TrackIds);
            UpdateResult updateResult = await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(p => p.Id == playlist.Id, updateDefinition);

            if (updateResult.ModifiedCount < 1)
            {
                throw new CustomException("Remove from Favorite Songs", 44, "Can't remove from your favorite songs");
            }
        }
    }
}
