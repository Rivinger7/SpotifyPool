using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using BusinessLogicLayer.Implement.CustomExceptions;
using Utility.Coding;
using BusinessLogicLayer.Enum.Microservices.Cloudinary;
using Business_Logic_Layer.Services_Interface.InMemoryCache;

namespace BusinessLogicLayer.Implement.Microservices.Cloudinaries
{
    public class CloudinaryService(Cloudinary cloudinary, ICacheCustom cache)
    {
        private readonly Cloudinary _cloudinary = cloudinary;
        private readonly ICacheCustom _cache = cache;

        public ImageUploadResult UploadImage(IFormFile imageFile, string tags = "AvatarUserProfile")
        {
            if (imageFile is null || imageFile.Length == 0)
            {
                throw new ArgumentNullCustomException(nameof(imageFile), "No file uploaded");
            }

            // Lấy đuôi file (File Extension)
            string? fileExtension = Path.GetExtension(imageFile.FileName).ToLower().TrimStart('.');

            // Kiểm tra nếu phần mở rộng có tồn tại trong enum ImageExtension
            if (!System.Enum.TryParse(fileExtension, true, out ImageExtension _))
            {
                throw new BadRequestCustomException("Unsupported file type");
            }

            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = "testing";

            // Hashing Metadata
            string hashedData = DataEncryptionExtensions.Encrypt($"image_{userID}");

            // Nếu người dùng đang ở khác muối giờ thì cách này hiệu quả hơn
            // Không nhất thiết phải là UTC+7 vì còn tùy thuộc theo hệ thống trên máy của người dùng
            string timestamp = DateTime.UtcNow.Ticks.ToString();

            using Stream? stream = imageFile.OpenReadStream();
            ImageUploadParams uploadParams = new()
            {
                File = new(imageFile.FileName, stream), // new FileDescription()
                //UseFilename = true,
                PublicId = $"{hashedData}_{timestamp}",
                UniqueFilename = false, // Đã custom nên không cần Unique từ Server nữa
                Tags = tags,
                Format = "webp",
                Overwrite = true
            };

            ImageUploadResult? uploadResult = _cloudinary.Upload(uploadParams);
            Console.WriteLine(uploadResult.JsonObj);

            return uploadResult;
        }

        public VideoUploadResult UploadVideo(IFormFile videoFile)
        {
            if (videoFile is null || videoFile.Length == 0)
            {
                throw new ArgumentNullCustomException(nameof(videoFile), "No file uploaded");
            }

            // Lấy đuôi file (File Extension)
            string? fileExtension = Path.GetExtension(videoFile.FileName).ToLower().TrimStart('.');

            // Kiểm tra nếu phần mở rộng có tồn tại trong enum ImageExtension
            if (!System.Enum.TryParse(fileExtension, true, out VideoExtension _))
            {
                throw new BadRequestCustomException("Unsupported file type");
            }

            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string userID = "testing";

            // Hashing Metadata
            string hashedData = DataEncryptionExtensions.Encrypt($"video_{userID}");

            // Nếu người dùng đang ở khác muối giờ thì cách này hiệu quả hơn
            // Không nhất thiết phải là UTC+7 vì còn tùy thuộc theo hệ thống trên máy của người dùng
            string timestamp = DateTime.UtcNow.Ticks.ToString();

            using Stream? stream = videoFile.OpenReadStream();
            VideoUploadParams uploadParams = new()
            {
                File = new(videoFile.FileName, stream), // new FileDescription()
                //UseFilename = true,
                PublicId = $"{hashedData}_{timestamp}",
                UniqueFilename = false, // Đã custom nên không cần Unique từ Server nữa
                Format = "mp4",
                Overwrite = true
            };

            VideoUploadResult? uploadResult = _cloudinary.Upload(uploadParams);
            Console.WriteLine(uploadResult.JsonObj);

            return uploadResult;
        }

        // Get Image from Server
        public GetResourceResult? GetImageResult(string publicID, bool isCache = false)
        {
            GetResourceResult? getResult = null;

            if (isCache)
            {
                getResult = _cache.GetOrSet(publicID, () => _cloudinary.GetResource(publicID));
            }
            else
            {
                getResult = _cloudinary.GetResource(publicID);
            }

            if ((int)getResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any Image with Public ID {publicID}");
            }

            return getResult;
        }

        // Get Video from Server
        public GetResourceResult? GetVideoResult(string publicID, bool isCache = false)
        {
            GetResourceResult? getResult = null;

            if (isCache)
            {
                getResult = _cache.GetOrSet(publicID, () => _cloudinary.GetResource(publicID));
            }
            else
            {
                getResult = _cloudinary.GetResource(publicID);
            }

            if ((int)getResult.StatusCode != StatusCodes.Status200OK)
            {
                throw new DataNotFoundCustomException($"Not found any Video with Public ID {publicID}");
            }

            return getResult;
        }

        // Update Image / Video from Client
        // Update và Upload dùng chung
        // Chỉ cần xử lý DB bên Upload là được

        // Delete Image from Server
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
                throw new DataNotFoundCustomException($"Not found any Image with Public ID {publicID}");
            }

            // Xóa cache nếu tồn tại bằng cách sử dụng hàm RemoveCache
            _cache.RemoveCache<GetResourceResult>(publicID); // Xóa cache cho kiểu GetResourceResult với khóa là publicID

            return deletionResult;
        }

        // Delete Video from Server
        public DeletionResult? DeleteVideo(string publicID)
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
                throw new DataNotFoundCustomException($"Not found any Image with Public ID {publicID}");
            }

            // Xóa cache nếu tồn tại bằng cách sử dụng hàm RemoveCache
            _cache.RemoveCache<GetResourceResult>(publicID); // Xóa cache cho kiểu GetResourceResult với khóa là publicID

            return deletionResult;
        }
    }
}
