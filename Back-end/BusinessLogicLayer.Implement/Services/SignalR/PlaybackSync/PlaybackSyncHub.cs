using BusinessLogicLayer.ModelView.Service_Model_Views.PlaybackState;
using DataAccessLayer.Implement.MongoDB.UOW;
using DataAccessLayer.Repository.Entities.SignalR;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.SignalR.PlaybackSync
{
    public class PlaybackSyncHub(UnitOfWork unitOfWork) : Hub
    {
        private readonly UnitOfWork _unitOfWork = unitOfWork;

        // Bộ nhớ RAM lưu trạng thái phát nhạc của từng user
        private static readonly ConcurrentDictionary<string, UserConnection> playbackStates = [];

        #region SignalR Base Connection Methods
        public override async Task OnConnectedAsync()
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            // Lấy thông tin connectionId từ Context
            string connectionId = Context.ConnectionId;

            // Lấy thông tin device từ Context
            string device = GetDeviceInfo();

            if (userId is null)
            {
                // Nếu không có thông tin user thì không thực hiện gì cả
                return;
            }

            // Projection
            ProjectionDefinition<UserConnection> projectionDefinition = Builders<UserConnection>.Projection
                .Include(x => x.ConnectionIds)
                .Include(x => x.IsConnected)
                .Include(x => x.Devices)
                .Include(x => x.LastConnected);

            // Kiểm tra xem userId đã tồn tại trong database chưa
            UserConnection userConnection = await _unitOfWork.GetCollection<UserConnection>().Find(x => x.UserId == userId)
                .Project<UserConnection>(projectionDefinition)
                .FirstOrDefaultAsync();

            // Lấy thời gian hiện tại
            DateTime currentTime = Util.GetUtcPlus7Time();

            if (userConnection is not null)
            {
                // Lưu userConnection vào bộ nhớ RAM
                playbackStates.AddOrUpdate(userId, userConnection, (key, existingValue) => userConnection);

                // Nếu ConnectionIds đã chứa connectionId thì không thực hiện gì cả
                if (userConnection.ConnectionIds.Contains(connectionId))
                {
                    await base.OnConnectedAsync();
                    return;
                }

                // Nếu ConnectionIds có nhiều hơn 5 phần tử thì xóa phần tử đầu tiên
                // 5 là số lượng connectionId tối đa mà một userId có thể có tại 1 thời điểm
                if (userConnection.ConnectionIds.Count > 5)
                {
                    userConnection.ConnectionIds.RemoveAt(0);
                }

                // Nếu Devices có nhiều hơn 5 phần tử thì xóa phần tử đầu tiên
                // 5 là số lượng các thiết bị mà một userId có thể sử dụng tại 1 thời điểm
                if (userConnection.Devices.Count > 5)
                {
                    userConnection.Devices.RemoveAt(0);
                }

                // Thêm connectionId mới vào ConnectionIds
                userConnection.ConnectionIds.Add(connectionId);

                // Thêm device mới vào Devices
                if (!userConnection.Devices.Contains(device))
                {
                    userConnection.Devices.Add(device);
                }

                // Nếu userId đã tồn tại thì cập nhật thông tin connection
                userConnection.IsConnected = true;
                userConnection.LastConnected = currentTime;

                // Cập nhật thông tin connection vào database
                UpdateDefinition<UserConnection> updateDefinition = Builders<UserConnection>.Update
                    .Set(x => x.ConnectionIds, userConnection.ConnectionIds)
                    .Set(x => x.IsConnected, userConnection.IsConnected)
                    .Set(x => x.Devices, userConnection.Devices)
                    .Set(x => x.LastConnected, userConnection.LastConnected);
                await _unitOfWork.GetCollection<UserConnection>().UpdateOneAsync(update => update.UserId == userId, updateDefinition);

                await base.OnConnectedAsync();
                return;
            }

            // Nếu userId chưa tồn tại thì tạo mới thông tin connection
            UserConnection newUserConnection = new()
            {
                ConnectionIds = [connectionId],
                UserId = userId,
                IsConnected = true,
                Devices = [device],
                CreatedTime = currentTime,
                LastConnected = currentTime,
            };

            // Lưu thông tin connection vào database
            await _unitOfWork.GetCollection<UserConnection>().InsertOneAsync(newUserConnection);

            // Lưu userConnection vào bộ nhớ RAM
            playbackStates.AddOrUpdate(userId, newUserConnection, (key, existingValue) => newUserConnection);

            await base.OnConnectedAsync();
            return;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.Identity?.Name;

            // Lấy thông tin connectionId từ Context
            string connectionId = Context.ConnectionId;

            // Lấy thông tin device từ Context
            string device = GetDeviceInfo();

            if (userId is null)
            {
                // Nếu không có thông tin user thì không thực hiện gì cả
                return;
            }

            // Lấy thời gian hiện tại
            DateTime currentTime = Util.GetUtcPlus7Time();

            // Projection
            ProjectionDefinition<UserConnection> projectionDefinition = Builders<UserConnection>.Projection
                .Include(x => x.ConnectionIds)
                .Include(x => x.IsConnected)
                .Include(x => x.Devices)
                .Include(x => x.LastDisconnected);

            UserConnection userConnection = await _unitOfWork.GetCollection<UserConnection>().Find(x => x.UserId == userId)
                .Project<UserConnection>(projectionDefinition)
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy thông tin userConnection thì không thực hiện gì cả
            if (userConnection is null)
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            // Loại bỏ ConnectionId khi tab hoặc thiết bị ngắt kết nối
            userConnection.ConnectionIds.Remove(connectionId);

            // Loại bỏ Device khi tab hoặc thiết bị ngắt kết nối
            userConnection.Devices.Remove(device);

            // Cập nhật trạng thái nếu không còn kết nối nào
            bool isConnected = userConnection.ConnectionIds.Count > 0;
            UpdateDefinition<UserConnection> updateDefinition = Builders<UserConnection>.Update
                .Set(x => x.ConnectionIds, userConnection.ConnectionIds)
                .Set(x => x.IsConnected, isConnected)
                .Set(x => x.LastDisconnected, currentTime);

            // Cập nhật thông tin connection vào database
            await _unitOfWork.GetCollection<UserConnection>().UpdateOneAsync(x => x.UserId == userId, updateDefinition);

            if(!isConnected)
            {
                // Xóa thông tin userConnection khỏi bộ nhớ RAM
                playbackStates.TryRemove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
            return;
        }

        private string GetDeviceInfo()
        {
            // Lấy thông tin thiết bị
            return Context.GetHttpContext()?.Request.Headers.UserAgent.ToString() ?? "Unknown";
        }
        #endregion

        public async Task GetPlaybackStateAsync()
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
                return;
            }

            // Kiểm tra trong bộ nhớ RAM xem user đã có thông tin PlaybackStateSync chưa
            if(playbackStates.TryGetValue(userId, out UserConnection? userConnection))
            {
                // Nếu có thì trả về thông tin PlaybackStateSync
                await Clients.Caller.SendAsync("SyncPlaybackState", userConnection);
                return;
            }
        }

        public async Task UpdatePlaybackSyncAsync(PlaybackStateSync playbackState)
        {
            // Lấy thông tin user từ Context
            string? userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Nếu không có thông tin user thì không thực hiện gì cả
            if (userId is null)
            {
                // Nên thông báo lỗi ở đây
                await Clients.Caller.SendAsync("ReceiveException", "Your session is limit, you must login again to create playlist!");
                return;
            }

            // Cập nhật thông tin PlaybackStateSync vào bộ nhớ RAM
            if(playbackStates.TryGetValue(userId, out UserConnection? userConnection))
            {
                bool hasChanged = userConnection.TrackId != playbackState.TrackId ||
                          Math.Abs(userConnection.CurrentTime - playbackState.CurrentTime) > 1.0 ||
                          userConnection.IsPlaying != playbackState.IsPlaying;

                userConnection.TrackId = playbackState.TrackId;
                userConnection.CurrentTime = playbackState.CurrentTime;
                userConnection.IsPlaying = playbackState.IsPlaying;
                userConnection.LastUpdated = Util.GetUtcPlus7Time();

                // Chỉ gửi phản hồi nếu có thay đổi đáng kể
                if (hasChanged)
                {
                    // Cập nhật Playback Sync của UserPlayback
                    await Clients.Caller.SendAsync("UpdateSyncPlaybackSync", userConnection);
                }
                return;
            }

            // Cập nhật Playback Sync của UserPlayback
            await Clients.Caller.SendAsync("UpdateSyncPlaybackSync", null);
            return;
        }
    }
}
