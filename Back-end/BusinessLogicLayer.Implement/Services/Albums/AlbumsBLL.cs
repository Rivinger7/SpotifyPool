using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Albums;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Albums.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Paging;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.Album;
using System.Security.Claims;
using Utility.Coding;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

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
        public async Task<IEnumerable<AlbumResponseModel>> GetAlbumsAsync(PagingRequestModel paging, AlbumFilterModel model)
        {
            FilterDefinition<Album> filter = Builders<Album>.Filter.Where(a => !a.DeletedTime.HasValue);

            if (!string.IsNullOrWhiteSpace(model.CreatedBy))
            {
                filter &= Builders<Album>.Filter.Eq(a => a.CreatedBy, model.CreatedBy);
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                filter &= Builders<Album>.Filter.Regex(a => a.Name, new BsonRegularExpression(model.Name, "i"));
                //"i": Không phân biệt chữ hoa, chữ thường.
                //MongoDB sẽ tìm tất cả các bản ghi mà Name chứa giá trị model.Name.
                //Ví dụ: model.Name = "rock" sẽ tìm được "Rock Music", "Hard Rock"...
            }

            if (model.ArtistIds.Any())
            {
                filter &= Builders<Album>.Filter.All(a => a.ArtistIds, model.ArtistIds);
            }
            if (model.ReleasedTime.HasValue)
            {
                filter &= Builders<Album>.Filter.Eq(a => a.ReleaseInfo.ReleasedTime, model.ReleasedTime);
            }
            if (model.Reason != null)
            {
                filter &= Builders<Album>.Filter.Eq(a => a.ReleaseInfo.Reason, model.Reason);
            }
            if (model.IsReleased.HasValue)
            {
                if (model.IsReleased.Value == true)
                {
                    filter &= Builders<Album>.Filter.Lte(a => a.ReleaseInfo.ReleasedTime, Util.GetUtcPlus7Time());
                    filter &= Builders<Album>.Filter.Eq(a => a.ReleaseInfo.Reason, ReleaseStatus.Official);
                } 
                else
                {
                    filter &= Builders<Album>.Filter.Gt(a => a.ReleaseInfo.ReleasedTime, Util.GetUtcPlus7Time());
                }
                
            }
            IFindFluent<Album, Album> albums = _unitOfWork.GetCollection<Album>().Find(filter);

            //Sort theo Name (Nếu có)
            if (model.IsSortByName.HasValue)
            {
                albums = model.IsSortByName.Value
                ? albums.SortBy(a => a.Name)   //Tăng dần if true
                    : albums.SortByDescending(a => a.Name); //Giảm dần if false
            }
            else
            {
                //Sort theo CreateTime giảm dần (albums mới tạo sẽ được đưa lên đầu)
                albums = albums.SortByDescending(a => a.CreatedTime);
            }
            IEnumerable<Album> result = await albums
                .Skip((paging.PageNumber - 1) * paging.PageSize) //Phân trang
                .Limit(paging.PageSize) //Giới hạn số lượng
                .ToListAsync();

            return _mapper.Map<IReadOnlyCollection<AlbumResponseModel>>(result);
        }
        public async Task<AlbumDetailResponseModel> GetAbumDetailByIdAsync(string albumId, bool? isSortByTrackName)
        {
            // Tạo SortDefinition trực tiếp dựa trên isSortByTrackName
            SortDefinition<TrackResponseModel>? sortDefinitions = isSortByTrackName.HasValue
                ? (isSortByTrackName.Value
                    ? Builders<TrackResponseModel>.Sort.Ascending(track => track.Name)
                    : Builders<TrackResponseModel>.Sort.Descending(track => track.Name))
                : null;

            //==============================================
            //1. XỬ LÝ THÔNG TIN CƠ BẢN ALBUM VÀ ARTIST TẠO
            //===============================================
            // Projection
            // Lấy tất cả field của Playlist nhưng chỉ một số field của User
            //ProjectionDefinition<ASAlbum> albumProjection = Builders<ASAlbum>.Projection
            //    .Exclude(a => a.Artist)
            //    .Include(a => a.Artist.Id)
            //    .Include(a => a.Artist.Name)
            //    .Include(a => a.Artist.Followers)
            //    .Include(a => a.Artist.Images);

            ProjectionDefinition<ASAlbum, AlbumDetailResponseModel> albumProjection = Builders<ASAlbum>.Projection.Expression(a => new AlbumDetailResponseModel
            {
                Info = new AlbumResponseModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Images = a.Images.Select(image => new ImageResponseModel()
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    ReleaseInfo = new ReleaseMetadataResponse()
                    {
                        ReleasedTime = a.ReleaseInfo.ReleasedTime.HasValue? a.ReleaseInfo.ReleasedTime.Value.ToString("HH:mm:ss dd/MM/yyyy") : null,
                        Reason = a.ReleaseInfo.Reason
                    }
                },
                CreatedBy = new ArtistResponseModel()
                {
                    Id = a.Artist.Id,
                    Name = a.Artist.Name,
                    Followers = a.Artist.Followers,
                    Images = a.Artist.Images.Select(image => new ImageResponseModel()
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    })
                },
                ArtistIds = a.ArtistIds,
                TrackIds = a.TrackIds
            });

            // Lookup
            AlbumDetailResponseModel album = await _unitOfWork.GetCollection<Album>()
                .Aggregate() //aggregation pipeline để thực hiện join dữ liệu giữa các collection.
                .Match(a => a.Id == albumId && !a.DeletedTime.HasValue) // Lọc trước, kiểm tả tồn tại
                .Lookup<Album, Artist, ASAlbum>(_unitOfWork.GetCollection<Artist>(),
                album => album.CreatedBy, // Field trong Playlist để match
                artist => artist.Id, // Field trong User để match
                result => result.Artist) // Lưu vào field Artist của ASAlbum
                .Unwind(result => result.Artist, new AggregateUnwindOptions<ASAlbum>()
                {
                    PreserveNullAndEmptyArrays = true
                    //Nếu Album không có Artist, nó vẫn giữ lại dữ liệu Playlist thay vì bị loại bỏ.
                }) //unwind để chuyển Artist từ dạng mảng sang object
                   //Sau unwind, nếu Artist có nhiều kết quả, nó sẽ tạo ra nhiều bản ghi Album (duplicate) mỗi cái chứa một Artist khác nhau.
                .Project(albumProjection) //Project: Chỉ lấy các thông tin cần thiết.
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Album with ID {albumId} does not exist");
            //==============================================
            //2. XỬ LÝ THÔNG TIN list Artist đc tag vô album
            //===============================================
            FilterDefinition<Artist> artistFilter = Builders<Artist>.Filter.In(a => a.Id, album.ArtistIds);
            // Chỉ lấy những thông tin cần thiết từ ASTrack : Track và Mappping sang TrackResponseModel
            // Mapping tracks to TrackResponseModel
            ProjectionDefinition<Artist, ArtistResponseModel> artistProjection = Builders<Artist>.Projection.Expression(a =>
                new ArtistResponseModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Followers = a.Followers,
                    Images = a.Images.Select(image => new ImageResponseModel()
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    })
                });
            IEnumerable<ArtistResponseModel> artists = await _unitOfWork.GetCollection<Artist>()
                .Find(artistFilter).Project(artistProjection).ToListAsync();
            // Lấy danh sách vô response
            album.Artists = artists;
            //==============================================
            //3. XỬ LÝ THÔNG TIN TRACK TRONG ALBUM (lấy tư album.TrackIds trên)
            //===============================================
            FilterDefinition<Track> trackFilter = Builders<Track>.Filter.In(track => track.Id, album.TrackIds);
            // Chỉ lấy những thông tin cần thiết từ ASTrack : Track và Mappping sang TrackResponseModel
            // Mapping tracks to TrackResponseModel
            ProjectionDefinition<ASTrack, TrackResponseModel> projectionDefinition = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    PreviewURL = track.StreamingUrl,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel()
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
            // Lấy thông tin Tracks với Artist
            // Lookup
           IAggregateFluent<TrackResponseModel> query = _unitOfWork.GetCollection<Track>()
                .Aggregate()
                .Match(trackFilter) // Match the pre custom filter
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(), // The foreign collection
                    track => track.ArtistIds, // The field in Track that are joining on
                    artist => artist.Id, // The field in Artist that are matching against
                    result => result.Artists) // The field in ASTrack to hold the matched artists
                .Project(projectionDefinition);
            // Chỉ gọi .Sort() nếu sortDefinitions không bị null và có giá trị
            if (sortDefinitions != null)
            {
                query = query.Sort(sortDefinitions);
            }
            // Lấy danh sách vô response
            album.Tracks = await query.ToListAsync();

            return album; 
        }


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
            Artist artist = await _unitOfWork.GetCollection<Artist>().Find(a => !a.DeletedTime.HasValue && a.UserId == userID).FirstOrDefaultAsync();
            string createBy = artist.Id;

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
                CreatedBy = createBy,
                ArtistIds = request.ArtistIds,
                Images = images,
                ReleaseInfo = new ReleaseMetadata()
                {
                    ReleasedTime = null,
                    Reason = ReleaseStatus.NotAnnounced
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
        public async Task AddTracksToAlbum(IEnumerable<string> trackIds, string albumId)
        {
            // Projection
            ProjectionDefinition<Album> projectionDefinition = Builders<Album>.Projection
                .Include(a => a.TrackIds);

            // Lấy thông tin playlist
            Album album = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .Project<Album>(projectionDefinition)
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("The album does not exist");
            IList<string> existTrackids = album.TrackIds;

            // Thêm track vào album
            foreach (var trackId in trackIds)
            {
                if (!existTrackids.Contains(trackId))
                {
                    album.TrackIds.Add(trackId);
                }
            }
            
            // Cập nhật album với danh sách TrackIds mới
            UpdateDefinition<Album> updateDefinition = Builders<Album>.Update.Set(a => a.TrackIds, album.TrackIds);
            await _unitOfWork.GetCollection<Album>().UpdateOneAsync(a => a.Id == albumId, updateDefinition);

            return;
        }
        public async Task RemoveTracksFromAlbum(IEnumerable<string> trackIds, string albumId)
        {
            // Projection
            ProjectionDefinition<Album> projectionDefinition = Builders<Album>.Projection
                .Include(a => a.TrackIds);

            // Lấy thông tin playlist
            Album album = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .Project<Album>(projectionDefinition)
                .FirstOrDefaultAsync() ?? throw new InvalidDataCustomException("The album does not exist");
            foreach (var trackId in trackIds)
            {
                album.TrackIds.Remove(trackId);
            }
            FilterDefinition<Album> filter = Builders<Album>.Filter.Eq(a => a.Id, albumId);
            UpdateDefinition<Album> update = Builders<Album>.Update.Set(a => a.TrackIds,
               album.TrackIds);

            await _unitOfWork.GetCollection<Album>().UpdateOneAsync(filter, update);
        }
        public async Task ReleaseAlbumAsync(string albumId, DateTime releaseTime)
        {
            if (releaseTime < Util.GetUtcPlus7Time())
            {
                throw new InvalidDataCustomException("Released Time must be >= now.");
            }

            FilterDefinition<Album> filter = Builders<Album>.Filter.Eq(a => a.Id, albumId);
            // Lấy album hiện tại
            Album existingAlbum = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Album with ID {albumId} does not exist");

            existingAlbum.ReleaseInfo.ReleasedTime = releaseTime;
            existingAlbum.ReleaseInfo.Reason = ReleaseStatus.Official;


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
        public async Task ChangeAlbumStatusAsync(string albumId, ReleaseStatus status)
        {
            // Lấy album hiện tại
            Album existingAlbum = await _unitOfWork.GetCollection<Album>()
                .Find(a => a.Id == albumId && !a.DeletedTime.HasValue)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Album with ID {albumId} does not exist.");

            FilterDefinition<Album> filter = Builders<Album>.Filter.Eq(a => a.Id, albumId);
            UpdateDefinition<Album> update = Builders<Album>.Update.Set(a => a.ReleaseInfo.Reason, status);

            await _unitOfWork.GetCollection<Album>().UpdateOneAsync(filter, update);
        }
    }
}
