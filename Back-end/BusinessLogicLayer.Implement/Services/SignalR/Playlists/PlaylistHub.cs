using AutoMapper;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.SignalR.Playlists
{
    public class PlaylistHub(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

        #region SignalR Base Connection Methods
        //public override async Task OnConnectedAsync()
        //{
        //    // Lấy thông tin user từ Context
        //    string? userId = Context.User?.Identity?.Name;

        //    // Lấy thông tin connectionId từ Context
        //    string connectionId = Context.ConnectionId;

        //    // Lấy thông tin device từ Context
        //    string device = GetDeviceInfo();

        //    if (userId is null)
        //    {
        //        // Nếu không có thông tin user thì không thực hiện gì cả
        //        return;
        //    }

        //    // Projection
        //    ProjectionDefinition<UserConnection> projectionDefinition = Builders<UserConnection>.Projection
        //        .Include(x => x.ConnectionIds)
        //        .Include(x => x.IsConnected)
        //        .Include(x => x.Devices)
        //        .Include(x => x.LastConnected);

        //    // Kiểm tra xem userId đã tồn tại trong database chưa
        //    UserConnection userConnection = await _unitOfWork.GetCollection<UserConnection>().Find(x => x.UserId == userId)
        //        .Project(projectionDefinition)
        //        .As<UserConnection>()
        //        .FirstOrDefaultAsync();

        //    // Lấy thời gian hiện tại
        //    DateTime currentTime = Util.GetUtcPlus7Time();

        //    if (userConnection is not null)
        //    {
        //        // Nếu ConnectionIds đã chứa connectionId thì không thực hiện gì cả
        //        if (userConnection.ConnectionIds.Contains(connectionId))
        //        {
        //            await base.OnConnectedAsync();
        //            return;
        //        }

        //        // Nếu ConnectionIds có nhiều hơn 5 phần tử thì xóa phần tử đầu tiên
        //        // 5 là số lượng connectionId tối đa mà một userId có thể có tại 1 thời điểm
        //        if (userConnection.ConnectionIds.Count > 5)
        //        {
        //            userConnection.ConnectionIds.RemoveAt(0);
        //        }

        //        // Nếu Devices có nhiều hơn 5 phần tử thì xóa phần tử đầu tiên
        //        // 5 là số lượng các thiết bị mà một userId có thể sử dụng tại 1 thời điểm
        //        if (userConnection.Devices.Count > 5)
        //        {
        //            userConnection.Devices.RemoveAt(0);
        //        }

        //        // Thêm connectionId mới vào ConnectionIds
        //        userConnection.ConnectionIds.Add(connectionId);

        //        // Thêm device mới vào Devices
        //        if (!userConnection.Devices.Contains(device))
        //        {
        //            userConnection.Devices.Add(device);
        //        }

        //        // Nếu userId đã tồn tại thì cập nhật thông tin connection
        //        userConnection.IsConnected = true;
        //        userConnection.LastConnected = currentTime;

        //        // Cập nhật thông tin connection vào database
        //        UpdateDefinition<UserConnection> updateDefinition = Builders<UserConnection>.Update
        //            .Set(x => x.ConnectionIds, userConnection.ConnectionIds)
        //            .Set(x => x.IsConnected, userConnection.IsConnected)
        //            .Set(x => x.Devices, userConnection.Devices)
        //            .Set(x => x.LastConnected, userConnection.LastConnected);
        //        await _unitOfWork.GetCollection<UserConnection>().UpdateOneAsync(update => update.UserId == userId, updateDefinition);

        //        await base.OnConnectedAsync();
        //        return;
        //    }

        //    // Nếu userId chưa tồn tại thì tạo mới thông tin connection
        //    UserConnection newUserConnection = new()
        //    {
        //        ConnectionIds = [connectionId],
        //        UserId = userId,
        //        IsConnected = true,
        //        Devices = [device],
        //        CreatedTime = currentTime,
        //        LastConnected = currentTime,
        //    };

        //    // Lưu thông tin connection vào database
        //    await _unitOfWork.GetCollection<UserConnection>().InsertOneAsync(newUserConnection);

        //    await base.OnConnectedAsync();
        //    return;
        //}

        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    // Lấy thông tin user từ Context
        //    string? userId = Context.User?.Identity?.Name;

        //    // Lấy thông tin connectionId từ Context
        //    string connectionId = Context.ConnectionId;

        //    // Lấy thông tin device từ Context
        //    string device = GetDeviceInfo();

        //    if (userId is null)
        //    {
        //        // Nếu không có thông tin user thì không thực hiện gì cả
        //        return;
        //    }

        //    // Lấy thời gian hiện tại
        //    DateTime currentTime = Util.GetUtcPlus7Time();

        //    // Projection
        //    ProjectionDefinition<UserConnection> projectionDefinition = Builders<UserConnection>.Projection
        //        .Include(x => x.ConnectionIds)
        //        .Include(x => x.IsConnected)
        //        .Include(x => x.Devices)
        //        .Include(x => x.LastDisconnected);

        //    UserConnection userConnection = await _unitOfWork.GetCollection<UserConnection>().Find(x => x.UserId == userId)
        //        .Project(projectionDefinition)
        //        .As<UserConnection>()
        //        .FirstOrDefaultAsync();

        //    // Nếu không tìm thấy thông tin userConnection thì không thực hiện gì cả
        //    if (userConnection is null)
        //    {
        //        await base.OnDisconnectedAsync(exception);
        //        return;
        //    }

        //    // Loại bỏ ConnectionId khi tab hoặc thiết bị ngắt kết nối
        //    userConnection.ConnectionIds.Remove(connectionId);

        //    // Loại bỏ Device khi tab hoặc thiết bị ngắt kết nối
        //    userConnection.Devices.Remove(device);

        //    // Cập nhật trạng thái nếu không còn kết nối nào
        //    bool isConnected = userConnection.ConnectionIds.Count > 0;
        //    UpdateDefinition<UserConnection> updateDefinition = Builders<UserConnection>.Update
        //        .Set(x => x.ConnectionIds, userConnection.ConnectionIds)
        //        .Set(x => x.IsConnected, isConnected)
        //        .Set(x => x.LastDisconnected, currentTime);

        //    // Cập nhật thông tin connection vào database
        //    await _unitOfWork.GetCollection<UserConnection>().UpdateOneAsync(x => x.UserId == userId, updateDefinition);

        //    await base.OnDisconnectedAsync(exception);
        //    return;
        //}

        //private string GetDeviceInfo()
        //{
        //    // Lấy thông tin thiết bị
        //    return Context.GetHttpContext()?.Request.Headers.UserAgent.ToString() ?? "Unknown";
        //}
        #endregion

        public async Task CreatePlaylistAsync(PlaylistRequestModel playlistRequestModel)
        {
            if (string.IsNullOrWhiteSpace(playlistRequestModel.PlaylistName))
            {
                await Clients.Caller.SendAsync("ReceiveException", "Playlist name is required");
                return;
            }

            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
                return;
            }

            // Kiểm tra xem playlist đã tồn tại chưa
            if (await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.UserID == userId && playlist.Name == playlistRequestModel.PlaylistName)
                .Project(playlist => playlist.Id)
                .AnyAsync())
            {
                await Clients.Caller.SendAsync("ReceiveException", "Playlist name already exists");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Tạo hình ảnh cho playlist
            List<Image> images = [];

            // Nếu có file hình ảnh thì upload lên Cloudinary
            // Gọi 3 lần để tạo 3 kích thước ảnh khác nhau
            if (playlistRequestModel.ImageFile != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh
                IEnumerable<int> sizes = [640, 300, 64];
                // Kích thước ảnh cố định
                int fixedSize = 300;

                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(playlistRequestModel.ImageFile, ImageTag.Playlist, rootFolder: "Image", fixedSize, fixedSize);

                // Tạo 3 kích thước ảnh khác nhau nhưng cùng một URL với kích thước cố định
                foreach (int size in sizes)
                {
                    images.Add(new()
                    {
                        URL = uploadResult.SecureUrl.AbsoluteUri,
                        Height = size,
                        Width = size
                    });
                }
            }
            // Nếu playlist là Favorite Songs thì sử dụng hình ảnh mặc định
            else
            {
                images =
                [
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779869/default-playlist-640_tsyulf.jpg",
                        Height = 640,
                        Width = 640
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779653/default-playlist-300_iioirq.png",
                        Height = 300,
                        Width = 300
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779699/default-playlist-64_gek7wt.png",
                        Height = 64,
                        Width = 64
                    }
                ];
            }

            // Tạo mới playlist
            Playlist playlist = new()
            {
                Name = playlistRequestModel.PlaylistName,
                TrackIds = [],
                UserID = userId,
                CreatedTime = Util.GetUtcPlus7Time(),
                Images = images
            };

            // Lưu thông tin playlist vào database
            await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);

            // Mapping thông tin playlist sang PlaylistsResponseModel
            PlaylistsResponseModel playlistResponseModel = _mapper.Map<PlaylistsResponseModel>(playlist);

            // Gửi thông báo đến cho user hiện tại
            await Clients.Caller.SendAsync("CreatePlaylistSuccessfully", playlistResponseModel);

            // Ngắt kết nối
            Context.Abort();

            return;
        }

        public async Task AddToFavoritePlaylistAsync(string trackId)
        {
            if (Context.User?.Identities is null)
            {
                await Clients.Caller.SendAsync("Unauthorized", "You do not have permission to access to SpotifyPool's Hub");
                Context.Abort();
                return;
            }

            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to add to favorite playlist!");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Projection
            ProjectionDefinition<Playlist> projectionDefinition = Builders<Playlist>.Projection
                .Include(playlist => playlist.TrackIds)
                .Include(playlist => playlist.Id);

            // Lấy thông tin playlist từ database
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(x => x.UserID == userId && x.Name == "Favorite Songs")
                .Project<Playlist>(projectionDefinition)
                .FirstOrDefaultAsync();

            // Nếu không tồn tại thì tạo mới playlist
            if (playlist is null)
            {
                playlist = new()
                {
                    Name = "Favorite Songs",
                    TrackIds =
                    [
                        new PlaylistTracksInfo()
                        {
                            TrackId = trackId,
                            AddedTime = Util.GetUtcPlus7Time()
                        }
                    ],
                    UserID = userId,
                    CreatedTime = Util.GetUtcPlus7Time(),
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

                // Lưu thông tin playlist vào database
                await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);

                // Tăng FavoriteSongsCount lên 1
                UpdateDefinition<Track> trackUpdateDefinition = Builders<Track>.Update.Inc(track => track.FavoriteCount, 1);
                await _unitOfWork.GetCollection<Track>().UpdateOneAsync(track => track.Id == trackId, trackUpdateDefinition);

                // Mapping thông tin playlist sang PlaylistsResponseModel
                PlaylistsResponseModel playlistResponseModel = _mapper.Map<PlaylistsResponseModel>(playlist);

                // Gửi thông báo đến cho user hiện tại
                await Clients.Caller.SendAsync("AddToNewFavoritePlaylistSuccessfully", playlistResponseModel);

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Nếu tồn tại thì thêm trackId vào playlist
            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Any(track => track.TrackId == trackId))
            {
                await Clients.Caller.SendAsync("ReceiveException", "The song has been already added to your Favorite Songs");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Thêm trackId vào danh sách TrackIds
            playlist.TrackIds.Add(new PlaylistTracksInfo
            {
                TrackId = trackId,
                AddedTime = Util.GetUtcPlus7Time()
            });

            string playlistId = playlist.Id;

            // Cập nhật playlist với danh sách TrackIds mới
            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(playlist => playlist.TrackIds, playlist.TrackIds);
            await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(playlist => playlist.Id == playlistId, updateDefinition);

            // Projection
            ProjectionDefinition<Track> trackProjection = Builders<Track>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Duration)
                .Include(x => x.StreamingUrl)
                .Include(x => x.ArtistIds)
                .Include(x => x.Images);

            // Lấy thông tin track từ database
            Track tracks = await _unitOfWork.GetCollection<Track>().Find(x => x.Id == trackId)
                .Project<Track>(trackProjection)
                .FirstOrDefaultAsync();

            // Mapping thông tin track sang TrackPlaylistResponseModel
            TrackPlaylistResponseModel trackPlaylistResponseModel = _mapper.Map<TrackPlaylistResponseModel>(tracks);

            // Thêm thông tin AddedTime vào TrackResponseModel
            trackPlaylistResponseModel.AddedTime = Util.GetUtcPlus7Time().ToString("yyyy-MM-dd");

            // Cập nhật playlist với danh sách TrackIds mới
            await Clients.Caller.SendAsync("AddToFavoritePlaylistSuccessfully", trackPlaylistResponseModel);

            // Ngắt kết nối
            Context.Abort();

            return;
        }

        public async Task AddToPlaylistAsync(string trackId, string? playlistId = null, string? playlistName = null)
        {
            // Nếu playlistName là Favorite Songs thì không thực hiện gì cả
            if (playlistName is not null && playlistName.Equals("Favorite Songs", StringComparison.Ordinal))
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Playlist name must not named Favorite Songs");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to add to playlist!");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Tạo Optional để kiểm tra playlistId có rỗng không
            MongoDB.Driver.Optional<string?> playlistIdOptional = Optional.Create(playlistId);

            // Lấy thông tin playlist từ database
            Playlist? playlist = playlistIdOptional.HasValue
                ? await _unitOfWork.GetCollection<Playlist>()
                .Find(playlist => playlist.Id == playlistIdOptional.Value)
                .Project<Playlist>(Builders<Playlist>.Projection.Include(playlist => playlist.Id))
                .FirstOrDefaultAsync()
                : null; // Trả về null nếu không có giá trị

            // Nếu không tồn tại thì tạo mới playlist
            if (playlist is null)
            {
                // Kiểm tra xem playlistName có rỗng không
                if (string.IsNullOrWhiteSpace(playlistName))
                {
                    await Clients.Caller.SendAsync("ReceiveException", "Playlist name is required");

                    // Ngắt kết nối
                    Context.Abort();

                    return;
                }

                // Nếu không tồn tại thì tạo mới playlist
                playlist = new()
                {
                    Name = playlistName,
                    TrackIds =
                    [
                        new PlaylistTracksInfo()
                        {
                            TrackId = trackId,
                            AddedTime = Util.GetUtcPlus7Time()
                        }
                    ],
                    UserID = userId,
                    CreatedTime = Util.GetUtcPlus7Time(),
                    Images =
                        [
                            new() {
                            URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779869/default-playlist-640_tsyulf.jpg",
                            Height = 640,
                            Width = 640,
                            },
                            new() {
                                URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779653/default-playlist-300_iioirq.png",
                                Height = 300,
                                Width = 300,
                            },
                            new() {
                                URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779699/default-playlist-64_gek7wt.png",
                                Height = 64,
                                Width = 64,
                            }
                        ]
                };

                // Lưu thông tin playlist vào database
                await _unitOfWork.GetCollection<Playlist>().InsertOneAsync(playlist);

                PlaylistsResponseModel playlistResponseModel = _mapper.Map<PlaylistsResponseModel>(playlist);

                // Gửi thông báo đến cho user hiện tại
                await Clients.Caller.SendAsync("AddToNewPlaylistSuccessfully", playlistResponseModel);

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Nếu tồn tại thì thêm trackId vào playlist
            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Any(track => track.TrackId == trackId))
            {
                await Clients.Caller.SendAsync("ReceiveException", "The song has been already added to your Playlist");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Ngày giờ hiện tại
            DateTime currentTime = Util.GetUtcPlus7Time();

            // Thêm trackId vào danh sách TrackIds
            playlist.TrackIds.Add(new PlaylistTracksInfo()
            {
                TrackId = trackId,
                AddedTime = currentTime
            });

            UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update.Set(playlist => playlist.TrackIds, playlist.TrackIds);
            await _unitOfWork.GetCollection<Playlist>().UpdateOneAsync(playlist => playlist.Id == playlistId, updateDefinition);

            // Projection
            ProjectionDefinition<Track> trackProjection = Builders<Track>.Projection
                .Include(x => x.Id)
                .Include(x => x.Name)
                .Include(x => x.Duration)
                .Include(x => x.StreamingUrl)
                .Include(x => x.ArtistIds)
                .Include(x => x.Images);

            // Lấy thông tin track từ database
            Track tracks = await _unitOfWork.GetCollection<Track>().Find(x => x.Id == trackId)
                .Project<Track>(trackProjection)
                .FirstOrDefaultAsync();

            // Mapping thông tin track sang TrackPlaylistResponseModel
            TrackPlaylistResponseModel trackPlaylistResponseModel = _mapper.Map<TrackPlaylistResponseModel>(tracks);

            // Thêm thông tin AddedTime vào TrackResponseModel
            trackPlaylistResponseModel.AddedTime = currentTime.ToString("yyyy-MM-dd");

            // Cập nhật playlist với danh sách TrackIds mới
            await Clients.Caller.SendAsync("AddToPlaylistSuccessfully", trackPlaylistResponseModel);

            // Ngắt kết nối
            Context.Abort();

            return;
        }

        public async Task DeletePlaylistAsync(string playlistId)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to delete playlist!");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Lấy thông tin playlist từ database
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(x => x.Id == playlistId)
                .Project<Playlist>(Builders<Playlist>.Projection.Include(playlist => playlist.Id))
                .FirstOrDefaultAsync();

            // Nếu không tồn tại thì thông báo lỗi
            if (playlist is null)
            {
                await Clients.Caller.SendAsync("ReceiveException", "Playlist not found");

                // Ngắt kết nối
                Context.Abort();

                return;
            }

            // Xóa playlist khỏi database
            await _unitOfWork.GetCollection<Playlist>().DeleteOneAsync(x => x.Id == playlistId);

            // Gửi thông báo đến cho user hiện tại
            await Clients.Caller.SendAsync("DeletePlaylistSuccessfully", playlistId);

            // Ngắt kết nối
            Context.Abort();

            return;
        }
    }
}
