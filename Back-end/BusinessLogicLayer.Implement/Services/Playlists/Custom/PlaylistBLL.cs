using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Custom;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Playlists.Custom
{
    public class PlaylistBLL(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IMapper mapper) : IPlaylist
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<PlaylistsResponseModel>> GetAllPlaylistsAsync()
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.Session.GetString("UserID");

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin Playlist
            IEnumerable<Playlist> playlists = await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.UserID == userID)
                .ToListAsync();

            // Mapping
            IEnumerable<PlaylistsResponseModel> playlistsResponse = _mapper.Map<IEnumerable<PlaylistsResponseModel>>(playlists);

            return playlistsResponse;
        }

        public async Task CreatePlaylistAsync(string playlistName)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Kiểm tra xem playlist đã tồn tại chưa
            if (await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.UserID == userID && playlist.Name == playlistName)
                .Project(playlist => playlist.Id)
                .AnyAsync())
            {
                throw new DataExistCustomException("The playlist has been already created");
            }

            // Tạo hình ảnh cho playlist
            List<Image> images = [];

            // Nếu playlist là Favorite Songs thì sử dụng hình ảnh mặc định
            if (playlistName.Equals("Favorite Songs", StringComparison.OrdinalIgnoreCase))
            {
                images =
                [
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-640_xnff8r.png",
                        Height = 640,
                        Width = 640
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-300_vitqvn.png",
                        Height = 300,
                        Width = 300
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1730189220/liked-songs-64_izigfw.png",
                        Height = 64,
                        Width = 64
                    }
                ];
            }

            // Tạo mới playlist
            Playlist playlist = new()
            {
                Name = playlistName,
                UserID = userID,
                CreatedTime = Util.GetUtcPlus7Time(),
                TrackIds = [],
                Images = images
            };

            // Thêm playlist vào DB
            await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);
        }

        public async Task AddToPlaylistAsync(string trackId, string playlistId)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Projection
            ProjectionDefinition<Playlist> projectionDefinition = Builders<Playlist>.Projection
                .Include(playlist => playlist.TrackIds);

            // Lấy thông tin playlist
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.Id == playlistId)
                .Project<Playlist>(projectionDefinition)
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("The playlist does not exist");

            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Any(track => track.TrackId == trackId))
            {
                throw new DataExistCustomException("The song has been already added to your Playlist");
            }

            // Thêm track vào playlist
            playlist.TrackIds.Add(new PlaylistTracksInfo
            {
                TrackId = trackId,
                AddedTime = Util.GetUtcPlus7Time()
            });

            // Cập nhật playlist với danh sách TrackIds mới
            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(playlist => playlist.TrackIds, playlist.TrackIds);
            await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(playlist => playlist.Id == playlistId, updateDefinition);

            return;
        }

        public async Task<IEnumerable<TrackPlaylistResponseModel>> GetTracksInPlaylistAsync(string playlistId)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Chỉ lấy những fields cần thiết từ Playlist
            ProjectionDefinition<Playlist> playlistProjection = Builders<Playlist>.Projection
                .Include(playlist => playlist.TrackIds);

            // Lấy thông tin Playlist
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.Id == playlistId)
                .Project<Playlist>(playlistProjection)
                .FirstOrDefaultAsync()
                ?? throw new DataNotFoundCustomException($"Not found any playlist with User {userID}");

            // Chỉ lấy những thông tin cần thiết từ ASTrack : Track
            ProjectionDefinition<ASTrack> astrackProjection = Builders<ASTrack>.Projection  // Project  
                .Include(ast => ast.Id)
                .Include(ast => ast.Name)
                .Include(ast => ast.Description)
                .Include(ast => ast.PreviewURL)
                .Include(ast => ast.Duration)
                .Include(ast => ast.Images)
                .Include(ast => ast.Artists);

            // Map track IDs and their added time
            Dictionary<string, DateTime> trackIdAddedTimeMap = playlist.TrackIds.ToDictionary(pti => pti.TrackId, pti => pti.AddedTime);
            HashSet<string> trackIdsSet = [.. trackIdAddedTimeMap.Keys]; // .ToHashSet()

            // Filter
            FilterDefinition<Track> trackFilter = Builders<Track>.Filter.In(track => track.Id, trackIdsSet);

            // Lấy thông tin Tracks với Artist
            IEnumerable<ASTrack> tracks = await _unitOfWork.GetRepository<ASTrack>().GetServeralTracksWithArtistAsync(trackFilter);

            // Mapping tracks with artists to TrackResponseModel
            // Thêm thông tin AddedTime vào TrackResponseModel
            IEnumerable<TrackPlaylistResponseModel> tracksResponse = tracks.Select(track =>
            {
                TrackPlaylistResponseModel trackResponse = _mapper.Map<TrackPlaylistResponseModel>(track);
                trackResponse.AddedTime = trackIdAddedTimeMap[track.Id].ToString("yyyy-MM-dd");
                return trackResponse;
            }).ToList();

            return tracksResponse;
        }

        public async Task RemoveFromPlaylistAsync(string trackId, string playlistId)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin playlist
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.Id == playlistId)
                .Project(playlist => playlist.TrackIds)
                .As<Playlist>()
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("The playlist does not exist");

            // Xóa track khỏi playlist
            playlist.TrackIds.RemoveAll(track => track.TrackId == trackId);
        }
    }
}
