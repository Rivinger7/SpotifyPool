using BusinessLogicLayer.Implement.CustomExceptions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using DataAccessLayer.Repository.Entities.SignalR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using SetupLayer.Enum.Services.Playlist;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.SignalR.Playlists
{
    public class PlaylistHub(IUnitOfWork unitOfWork) : Hub
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

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

        public async Task AddToFavoritePlaylistAsync(string trackId)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                return;
            }

            // Projection
            ProjectionDefinition<Playlist> projectionDefinition = Builders<Playlist>.Projection
                .Include(x => x.Id)
                .Include(x => x.TrackIds)
                .Include(x => x.UserID);

            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(x => x.UserID == userId && x.Name == PlaylistName.FavoriteSong.ToString())
                    .Project<Playlist>(projectionDefinition)
                    .FirstOrDefaultAsync();

            // Nếu không tồn tại thì tạo mới playlist
            if (playlist is null)
            {
                playlist = new()
                {
                    Name = PlaylistName.FavoriteSong.ToString(),
                    TrackIds =
                    [
                        new PlaylistTracksInfo
                        {
                            TrackId = trackId,
                            AddedTime = Util.GetUtcPlus7Time(),
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

                // Gửi thông báo đến cho user hiện tại
                await Clients.User(userId).SendAsync("ReceivePlaylist", "Testing");

                return;
            }

            // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
            if (playlist.TrackIds.Any(t => t.TrackId == trackId))
            {
                throw new DataExistCustomException("The song has been added to your Favorite Song");
            }

            // Thêm trackId vào danh sách TrackIds
            playlist.TrackIds.Add(new PlaylistTracksInfo
            {
                TrackId = trackId,
                AddedTime = Util.GetUtcPlus7Time()
            });

            // Cập nhật playlist với danh sách TrackIds mới
            await Clients.User(userId).SendAsync("ReceiveNotification", $"New song added to playlist: ");

            return;
        }

        public async Task CreateCustomPlaylistAsync(string playlistName)
        {
            if (string.IsNullOrEmpty(playlistName))
            {
                throw new DataNotFoundCustomException("Playlist name is required");
            }

            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                return;
            }

            // Lấy thông tin playlist từ database
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(x => x.UserID == userId && x.Name == playlistName)
                .Project(playlist => playlist.Name)
                .As<Playlist>()
                .FirstOrDefaultAsync();

            // Nếu tồn tại thì thông báo lỗi
            if (playlist is not null)
            {
                throw new DataExistCustomException("Playlist name already exists");
            }

            // Tạo mới playlist
            playlist = new()
            {
                Name = playlistName,
                TrackIds = [],
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

            // Gửi thông báo đến cho user hiện tại
            await Clients.User(userId).SendAsync("ReceiveNotification", $"New playlist created: ");
            return;
        }

        public async Task AddToCustomPlaylistAsync(string trackId, string? playlistId = null, string? playlistName = null)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                return;
            }

            // Projection
            ProjectionDefinition<Playlist> projectionDefinition = Builders<Playlist>.Projection
                .Include(x => x.Id)
                .Include(x => x.TrackIds)
                .Include(x => x.UserID);

            // Lấy thông tin playlist từ database
            Playlist playlist = await _unitOfWork.GetCollection<Playlist>().Find(x => x.Id == playlistId)
                .Project(playlist => playlist.Id)
                .As<Playlist>()
                .FirstOrDefaultAsync();

            // Nếu tồn tại thì thêm trackId vào playlist
            if (playlist is not null)
            {
                // Chỉ thêm trackID nếu chưa tồn tại để tránh trùng lặp
                if (playlist.TrackIds.Any(track => track.TrackId == trackId))
                {
                    throw new DataExistCustomException("The song has been already added to your Playlist");
                }

                // Thêm trackId vào danh sách TrackIds
                playlist.TrackIds.Add(new PlaylistTracksInfo()
                {
                    TrackId = trackId,
                    AddedTime = Util.GetUtcPlus7Time()
                });

                // Cập nhật playlist với danh sách TrackIds mới
                await Clients.User(userId).SendAsync("ReceiveNotification", $"New song added to playlist: ");

                return;
            }

            // Kiểm tra xem playlistName có tồn tại không
            if (string.IsNullOrEmpty(playlistName))
            {
                throw new DataNotFoundCustomException("Playlist name is required");
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
                    }],
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

            // Gửi thông báo đến cho user hiện tại
            //await Clients.User(userId).SendAsync("AddToPlaylistAsync", playlistId, trackId);
            await Clients.User(userId).SendAsync("ReceiveNotification", $"New song added to playlist: ");

            return;
        }
    }
}
