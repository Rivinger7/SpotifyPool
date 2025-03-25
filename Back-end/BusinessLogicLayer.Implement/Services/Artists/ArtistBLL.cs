using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Artists;
using BusinessLogicLayer.Interface.Services_Interface.JWTs;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Authentication.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Request;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.User;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Artists
{
    public class ArtistBLL(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService, IHttpContextAccessor httpContextAccessor, IJwtBLL jwtBLL) : IArtist
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IJwtBLL _jwtBLL = jwtBLL;

        public async Task CreateArtist(ArtistRequest artistRequest)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Thêm role user vào user
            UpdateDefinition<User> updateDefinition = Builders<User>.Update.AddToSet(u => u.Roles, UserRole.Artist);
            UpdateResult updateResult = await _unitOfWork.GetCollection<User>().UpdateOneAsync(u => u.Id == userID, updateDefinition);

            // Kiểm tra role đã được thêm vào user chưa
            if (updateResult.ModifiedCount < 1)
            {
                throw new InternalServerErrorCustomException("Can't add user role to user!");
            }

            // Kiểm tra xem account đã là nghệ sĩ chưa
            Artist artist = await _unitOfWork.GetCollection<Artist>().Find(a => a.UserId == userID).FirstOrDefaultAsync();
            if (artist is not null)
            {
                throw new DataExistCustomException("You are already have an user account!");
            }

            // Khởi tạo danh sách hình ảnh
            List<Image> images = [];

            // Nếu có file hình ảnh thì upload lên Cloudinary
            // Gọi 3 lần để tạo 3 kích thước ảnh khác nhau
            if (artistRequest.ImageFile != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh
                IEnumerable<int> sizes = [640, 300, 64];
                // Kích thước ảnh cố định
                int fixedSize = 300;

                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(artistRequest.ImageFile, ImageTag.Artist, rootFolder: "Image", fixedSize, fixedSize);

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

            // Tạo mới account nghệ sĩ
            artist = new()
            {
                Name = artistRequest.Name,
                UserId = userID,
                Images = images,
                CreatedTime = Util.GetUtcPlus7Time(),
            };

            // Lưu thông tin nghệ sĩ vào database
            await _unitOfWork.GetCollection<Artist>().InsertOneAsync(artist);
        }

        public async Task<AuthenticatedUserInfoResponseModel> SwitchToUserProfile()
        {
            // Lấy UserId từ phiên người dùng
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Lấy thông tin user
            User user = await _unitOfWork.GetCollection<User>().Find(a => a.Id == userID).FirstOrDefaultAsync() ?? throw new DataNotFoundCustomException("Not found any user!");

            // Claim list
            IEnumerable<Claim> claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, UserRole.Customer.ToString()),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim("Avatar", user.Images[0].URL ?? throw new DataNotFoundCustomException("Not found any images"))
                ];

            // Gọi phương thức để tạo access token và refresh token từ danh sách claim và thông tin người dùng
            _jwtBLL.GenerateAccessToken(claims, userID, out string accessToken, out string refreshToken);

            // Tạo cookie
            CookieOptions cookieOptions = new()
            {
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(7)
            };

            // Thêm refresh token vào cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Append("SpotifyPool_RefreshToken", refreshToken, cookieOptions);

            // Tạo thông tin người dùng đã xác thực
            AuthenticatedUserInfoResponseModel authenticatedUserInfoResponseModel = new()
            {
                AccessToken = accessToken,
                Id = user.Id.ToString(),
                Name = user.DisplayName,
                Role = [UserRole.Customer.ToString()],
                Avatar = [user.Images[0].URL]
            };

            return authenticatedUserInfoResponseModel;
        }

        public async Task<ArtistResponseModel> GetArtistByIdAsync(string artistId)
        {
            // Projection
            ProjectionDefinition<Artist, ArtistResponseModel> artistProjection = Builders<Artist>.Projection.Expression(artist =>
                new ArtistResponseModel
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Followers = artist.Followers,
                    Images = artist.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    })
                });

            // Lấy thông tin nghệ sĩ
            ArtistResponseModel artistResponseModel = await _unitOfWork.GetCollection<Artist>().Find(a => a.Id == artistId)
                .Project(artistProjection)
                .FirstOrDefaultAsync();
            return artistResponseModel;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetOwnTracks(int offset, int limit)
        {
            // Lấy UserId từ phiên người dùng
            string? artistId = _httpContextAccessor.HttpContext?.User.FindFirst("ArtistId")?.Value;

            // Kiểm tra UserId
            if (string.IsNullOrEmpty(artistId))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.StreamingUrl,
                    Duration = track.Duration,
                    UploadDate = track.UploadDate,
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
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Lấy danh sách track của artist

            // Lấy tracks có streamingUrl != null và isPlayable = true
            FilterDefinition<Track> trackFilter = Builders<Track>.Filter.And(
                Builders<Track>.Filter.Ne(t => t.StreamingUrl, null),
                Builders<Track>.Filter.Eq(t => t.Restrictions.IsPlayable, true),
                Builders<Track>.Filter.AnyEq(t => t.ArtistIds, artistId)
            );

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Match(trackFilter)
                .Skip((offset - 1) * limit)
                .Limit(limit)
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(trackWithArtistProjection)
                .ToListAsync();

            // lấy AlbumIds mỗi track
            IEnumerable<Album> allAlbums = await _unitOfWork.GetCollection<Album>().Find(a => !a.DeletedTime.HasValue).ToListAsync();
            foreach (var track in tracksResponseModel)
            {
                IEnumerable<string> albumIds = allAlbums.Where(a => a.TrackIds.Contains(track.Id)).Select(a => a.Id);
                track.AlbumIds = albumIds;
            }

            return tracksResponseModel;
        }

        public async Task<IEnumerable<TrackResponseModel>> GetTracksByArtistId(string artistId, int offset, int limit)
        {
            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.StreamingUrl,
                    Duration = track.Duration,
                    UploadDate = track.UploadDate,
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
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Lấy danh sách track của artist
            // Lấy tracks có streamingUrl != null và isPlayable = true
            FilterDefinition<Track> trackFilter = Builders<Track>.Filter.And(
                Builders<Track>.Filter.Ne(t => t.StreamingUrl, null),
                Builders<Track>.Filter.Eq(t => t.Restrictions.IsPlayable, true),
                Builders<Track>.Filter.AnyEq(t => t.ArtistIds, artistId)
            );

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Match(trackFilter)
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

        public async Task UpdateArtistProfile(string artistId, EditProfileRequestModel request)
        {
            Artist artist = await _unitOfWork.GetCollection<Artist>().Find(a => !a.DeletedTime.HasValue && a.Id == artistId).FirstOrDefaultAsync();
            artist.Name = request.DisplayName;
            artist.LastUpdatedTime = Util.GetUtcPlus7Time();
            if (request.Image != null)
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh cố định
                int fixedSize = 300;
                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(request.Image, ImageTag.Album, rootFolder: "Image", fixedSize, fixedSize);
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
                artist.Images = images;
            }

            FilterDefinition<Artist> filter = Builders<Artist>.Filter.Eq(a => a.Id, artist.Id);
            //Chuyển thành BsonDocument để cập nhật, loại bỏ _id
            BsonDocument bsonDoc = artist.ToBsonDocument();
            bsonDoc.Remove("_id");

            //Tạo UpdateDefinition từ BsonDocument
            BsonDocument update = new BsonDocument("$set", bsonDoc);
            UpdateResult result = await _unitOfWork.GetCollection<Artist>()
                .UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException($"Artist with ID {artist.Id} does not exist.");
            }
        }
    }
}
