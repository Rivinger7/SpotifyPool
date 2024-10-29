using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using BusinessLogicLayer.Implement.CustomExceptions;
using Utility.Coding;
using Business_Logic_Layer.Services_Interface.InMemoryCache;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using System.Web;
using SetupLayer.Enum.Microservices.Cloudinary;

namespace BusinessLogicLayer.Implement.Microservices.Cloudinaries
{
    public class CloudinaryService(Cloudinary cloudinary, ICacheCustom cache, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        private readonly Cloudinary _cloudinary = cloudinary;
        private readonly ICacheCustom _cache = cache;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public ImageUploadResult UploadImage(IFormFile imageFile, ImageTag imageTag, string rootFolder = "Image", int? height = null, int? width = null)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            if (imageFile is null || imageFile.Length == 0)
            {
                throw new ArgumentNullCustomException(nameof(imageFile), "No file uploaded");
            }

            #region Kiểm tra bằng đuôi file (.ext)
            //// Lấy đuôi file (File Extension)
            //string? fileExtension = Path.GetExtension(imageFile.FileName).ToLower().TrimStart('.');

            //// Kiểm tra nếu phần mở rộng có tồn tại trong enum ImageExtension
            //if (!System.Enum.TryParse(fileExtension, true, out ImageExtension _))
            //{
            //    throw new BadRequestCustomException("Unsupported file type");
            //}
            #endregion

            // Kiểm tra bằng content-type (image/webp)
            string fileType = imageFile.ContentType.Split('/').First();
            if (fileType != "image")
            {
                throw new BadRequestCustomException("Unsupported file type");
            }

            string currentFolder = $"{rootFolder}/{imageTag}";

            // Hashing Metadata
            string hashedData = DataEncryptionExtensions.Encrypt($"image_{imageTag}_{userID}");

            // Nếu người dùng đang ở khác muối giờ thì cách này hiệu quả hơn
            // Không nhất thiết phải là UTC+7 vì còn tùy thuộc theo hệ thống trên máy của người dùng
            //string timestamp = DateTime.UtcNow.Ticks.ToString();
            // Không cần timestamp để giữ tính nhất quán nếu có update

            // Open Read Stream
            using Stream? stream = imageFile.OpenReadStream();

            // Nếu không đặt giá trị height và width thì sẽ lấy mặc định
            if(height is null || width is null)
            {
                (height, width) = Util.GetImageDimensions(stream);
            }

            // Khởi tạo các thông số cần thiết cho Image
            ImageUploadParams uploadParams = new()
            {
                AssetFolder = currentFolder, // Dùng assetFolder sẽ không yêu cầu publicPrefixID
                File = new(imageFile.FileName, stream), // new FileDescription()
                //UseFilename = true,
                PublicId = $"{Uri.EscapeDataString(hashedData)}",
                DisplayName = imageFile.FileName,
                UniqueFilename = false, // Đã custom nên không cần Unique từ Server nữa
                Tags = imageTag.ToString(),
                Format = "webp",
                Overwrite = true,
                Transformation = new Transformation().Width(width).Height(height).Crop("fill"),
            };

            // Kết quả Response
            ImageUploadResult? uploadResult = _cloudinary.Upload(uploadParams);

            if ((int)uploadResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new CustomException("Error", (int)uploadResult.StatusCode, uploadResult.Status);
            }

            Console.WriteLine(uploadResult.JsonObj);

            return uploadResult;
        }

        public VideoUploadResult UploadTrack(IFormFile trackFile, AudioTagParent audioTagParent, AudioTagChild audioTagChild, string rootFolder = "Audio")
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = _httpContextAccessor.HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            if (trackFile is null || trackFile.Length == 0)
            {
                throw new ArgumentNullCustomException(nameof(trackFile), "No file uploaded");
            }

            #region Kiểm tra bằng đuôi file (.ext)
            //// Lấy đuôi file (File Extension)
            //string? fileExtension = Path.GetExtension(trackFile.FileName).ToLower().TrimStart('.');

            //// Kiểm tra nếu phần mở rộng có tồn tại trong enum ImageExtension
            //if (!System.Enum.TryParse(fileExtension, true, out VideoExtension _))
            //{
            //    throw new BadRequestCustomException("Unsupported file type");
            //}
            #endregion

            // Kiểm tra bằng content-type
            string fileType = trackFile.ContentType.Split('/').First();
            switch (fileType)
            {
                case "audio": break;
                case "video": break;
                default: throw new BadRequestCustomException("Unsupported file type");
            }

            // Hashing Metadata
            string hashedData = DataEncryptionExtensions.Encrypt($"track_{audioTagChild}_{userID}");

            // Nếu người dùng đang ở khác muối giờ thì cách này hiệu quả hơn
            // Không nhất thiết phải là UTC+7 vì còn tùy thuộc theo hệ thống trên máy của người dùng
            //string timestamp = DateTime.UtcNow.Ticks.ToString();
            // Không cần timestamp để giữ tính nhất quán nếu có update

            string currentFolder = $"{rootFolder}/{audioTagParent}/{audioTagChild}";

            using Stream? stream = trackFile.OpenReadStream();
            VideoUploadParams uploadParams = new()
            {
                AssetFolder = currentFolder, // Dùng assetFolder sẽ không yêu cầu publicPrefixID
                File = new(trackFile.FileName, stream), // new FileDescription()
                //UseFilename = true,
                PublicId = $"{Uri.EscapeDataString(hashedData)}",
                DisplayName = trackFile.FileName,
                UniqueFilename = false, // Đã custom nên không cần Unique từ Server nữa
                Format = "mp3",
                Overwrite = true
            };

            VideoUploadResult? uploadResult = _cloudinary.Upload(uploadParams);

            if ((int)uploadResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new CustomException("Error", (int)uploadResult.StatusCode, uploadResult.Status);
            }

            Console.WriteLine(uploadResult.JsonObj);

            return uploadResult;
        }

        // Get ImageResponseModel from Server
        public GetResourceResult? GetImageResult(string publicID, bool isCache = false)
        {
            GetResourceResult? getResult = null;

            GetResourceParams? getResourceParams = new(publicID)
            {
                ResourceType = ResourceType.Image,
            };

            if (isCache)
            {
                getResult = _cache.GetOrSet(publicID, () => _cloudinary.GetResource(getResourceParams));
            }
            else
            {
                getResult = _cloudinary.GetResource(getResourceParams);
            }

            if ((int)getResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any ImageResponseModel with Public ID {publicID}");
            }

            return getResult;
        }

        // Get Video from Server
        public GetResourceResult? GetTrackResult(string publicID, bool isCache = false)
        {
            GetResourceResult? getResult = null;

            GetResourceParams getResourceParams = new(publicID)
            {
                ResourceType = ResourceType.Video  // Explicitly set the resource type to video
            };

            if (isCache)
            {
                getResult = _cache.GetOrSet(publicID, () => _cloudinary.GetResource(getResourceParams));
            }
            else
            {
                getResult = _cloudinary.GetResource(getResourceParams);
            }

            if ((int)getResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any Track with Public ID {publicID}");
            }

            return getResult;
        }

        // Update ImageResponseModel / Video from Client
        // Update và Upload dùng chung
        // Chỉ cần xử lý DB bên Upload là được
        // Vì chỉ cần upload trùng với publicID cũ là nó sẽ ghi đè lên thay vì tạo mới

        // Delete ImageResponseModel from Server
        public DeletionResult? DeleteImage(string publicID)
        {
            DeletionParams deletionParams = new(publicID)
            {
                //PublicId = publicID, // Không cần vì hàm này yêu cầu có tham số publicID nên không cần khởi tạo nữa
                ResourceType = ResourceType.Image,
                Type = "upload"
            };

            DeletionResult? deletionResult = _cloudinary.Destroy(deletionParams);

            if ((int)deletionResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any ImageResponseModel with Public ID {publicID}");
            }

            // Xóa cache nếu tồn tại bằng cách sử dụng hàm RemoveCache
            //_cache.RemoveCache<GetResourceResult>(publicID); // Xóa cache cho kiểu GetResourceResult với khóa là publicID

            return deletionResult;
        }

        // Delete Video from Server
        public DeletionResult? DeleteTrack(string publicID)
        {
            DeletionParams deletionParams = new(publicID)
            {
                //PublicId = publicID, // Không cần vì hàm này yêu cầu có tham số publicID nên không cần khởi tạo nữa
                ResourceType = ResourceType.Video,
                Type = "upload"
            };

            DeletionResult deletionResult = _cloudinary.Destroy(deletionParams);

            if ((int)deletionResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any ImageResponseModel with Public ID {publicID}");
            }

            // Xóa cache nếu tồn tại bằng cách sử dụng hàm RemoveCache
            //_cache.RemoveCache<GetResourceResult>(publicID); // Xóa cache cho kiểu GetResourceResult với khóa là publicID

            return deletionResult;
        }
    }
}
