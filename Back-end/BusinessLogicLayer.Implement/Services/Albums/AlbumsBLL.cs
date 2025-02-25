using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Albums;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.Album;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Albums
{
    public class AlbumsBLL : IAlbums
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;

        public AlbumsBLL(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = contextAccessor;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }
        //public async Task<IEnumerable<AlbumResponseModel>> GetAlbumsAsync()
        //{
        //    // UserID lấy từ phiên người dùng có thể là FE hoặc BE
        //    string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    // Kiểm tra UserId
        //    if (string.IsNullOrEmpty(userID))
        //    {
        //        throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
        //    }

        //    // Projection
        //    ProjectionDefinition<Playlist> playlistProjection = Builders<Playlist>.Projection
        //        .Include(playlist => playlist.Id)
        //        .Include(playlist => playlist.Name)
        //        .Include(playlist => playlist.Images);

        //    // Lấy thông tin Playlist
        //    IEnumerable<Playlist> playlists = await _unitOfWork.GetCollection<Playlist>()
        //        .Find(playlist => playlist.UserID == userID)
        //        .Project<Playlist>(playlistProjection)
        //        .ToListAsync();

        //    // Mapping
        //    IEnumerable<PlaylistsResponseModel> playlistsResponse = _mapper.Map<IEnumerable<PlaylistsResponseModel>>(playlists);

        //    return playlistsResponse;
        //}
        public async Task CreateAlbumAsync(AlbumRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidDataCustomException("Album name is required.");
            }
            //if (request.ReleasedTime.HasValue && request.ReleasedTime <= Util.GetUtcPlus7Time())
            //{
            //    throw new InvalidDataCustomException("Released Time must be > now.");
            //}

            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Kiểm tra xem album đã tồn tại chưa
            if (await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Name == request.Name && !a.DeletedTime.HasValue)
                .AnyAsync())
            {
                throw new DataExistCustomException("This album already exists.");
            }

            // Tạo hình ảnh cho album
            List<Image> images = [];
            // Kích thước ảnh
            IEnumerable<int> sizes = [640, 300, 64];
            string? imageUrl = null;
            // Nếu có file hình ảnh thì upload lên Cloudinary
            if (request.ImageFile != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh cố định
                int fixedSize = 300;
                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(request.ImageFile, ImageTag.Album, rootFolder: "Image", fixedSize, fixedSize);
                imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                
            }
            // Nếu không có thì sử dụng hình ảnh mặc định
            else
            {
                imageUrl = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779653/default-playlist-300_iioirq.png";
            }
            // Tạo 3 kích thước ảnh khác nhau nhưng cùng một URL với kích thước cố định
            foreach (int size in sizes)
            {
                images.Add(new()
                {
                    URL = imageUrl,
                    Height = size,
                    Width = size
                });
            }

            // Tạo mới album
            Album album = new()
            {
                Name = request.Name,
                Description = request.Description,
                CreatedTime = Util.GetUtcPlus7Time(),
                CreatedBy = userID,
                ArtistIds = request.ArtistIds,
                Images = images,
                ReleaseInfo = new ReleaseMetadata()
                {
                    ReleasedTime = request.ReleasedTime,
                    Reason = request.Reason
                }
            };

            // Thêm playlist vào DB
            await _unitOfWork.GetCollection<Album>().InsertOneAsync(album);
        }

        public async Task UpdateAlbumAsync(string albumId, AlbumRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new InvalidDataCustomException("Album name is required.");
            }
            //if (request.ReleasedTime.HasValue && request.ReleasedTime <= Util.GetUtcPlus7Time())
            //{
            //    throw new InvalidDataCustomException("Released Time must be > now.");
            //}

            FilterDefinition<Album> filter = Builders<Album>.Filter.Eq(a => a.Id, albumId);
            // Lấy album hiện tại
            Album existingAlbum = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Album with ID {albumId} does not exist");

            existingAlbum.Name = request.Name;
            existingAlbum.Description = request.Description;
            existingAlbum.ArtistIds = request.ArtistIds;
            existingAlbum.ReleaseInfo.ReleasedTime = request.ReleasedTime;
            existingAlbum.ReleaseInfo.Reason = request.Reason;
            existingAlbum.LastUpdatedTime = Util.GetUtcPlus7Time();
            if (request.ImageFile != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh cố định
                int fixedSize = 300;
                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(request.ImageFile, ImageTag.Album, rootFolder: "Image", fixedSize, fixedSize);
                string imageUrl = uploadResult.SecureUrl.AbsoluteUri;
                // Tạo hình ảnh cho album
                List<Image> images = [];
                // Kích thước ảnh
                IEnumerable<int> sizes = [640, 300, 64];
                // Tạo 3 kích thước ảnh khác nhau nhưng cùng một URL với kích thước cố định
                foreach (int size in sizes)
                {
                    images.Add(new()
                    {
                        URL = imageUrl,
                        Height = size,
                        Width = size
                    });
                }
                existingAlbum.Images = images;
            }
            

            //Chuyển thành BsonDocument để cập nhật, loại bỏ _id
            BsonDocument bsonDoc = existingAlbum.ToBsonDocument();
            bsonDoc.Remove("_id");

            //Tạo UpdateDefinition từ BsonDocument
            BsonDocument update = new BsonDocument("$set", bsonDoc);
            UpdateResult result = await _unitOfWork.GetCollection<Album>()
                .UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Album with ID {albumId} does not exist.");
            }
        }
        public async Task DeleteAlbumAsync(string albumId)
        { 
            // Lấy album hiện tại
            Album existingAlbum = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Album with ID {albumId} does not exist.");

            FilterDefinition<Album> filter = Builders<Album>.Filter.Eq(a => a.Id, albumId);
            UpdateDefinition<Album> update = Builders<Album>.Update.Set(a => a.DeletedTime,
               Util.GetUtcPlus7Time());

            await _unitOfWork.GetCollection<Album>().UpdateOneAsync(filter, update);
        }
    }
}
