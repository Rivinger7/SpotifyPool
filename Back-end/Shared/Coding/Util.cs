using System.Globalization;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.Serialization;

namespace Utility.Coding
{
    public static class Util
    {
        public static string ConvertVnString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Chuẩn hóa chuỗi Unicode về dạng chuẩn (dấu tách biệt)
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            // Loại bỏ các ký tự có dấu
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Chuyển đổi về dạng không dấu, chuyển thường và loại bỏ khoảng trắng thừa
            var result = stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "");

            return result;
        }
        public static string GenerateRandomKey(int length = 5)
        {
            var pattern = @"asdasdasdasdasQSADAGKJPOK!";
            var sb = new StringBuilder();
            var random = new Random(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[random.Next(0, pattern.Length)]);
            }

            return sb.ToString();
        }

        public static string GenerateRandomKey(string data, int length = 5)
        {
            var pattern = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" + data;
            var sb = new StringBuilder();
            var random = new Random(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[random.Next(0, pattern.Length)]);
            }

            return sb.ToString();
        }

        public static DateTime GetUtcPlus7Time()
        {
            #region Chỉ chạy được trên local nếu publish thì sẽ lỗi
            // Get the current UTC time
            //DateTime utcNow = DateTime.UtcNow;

            //// Define the UTC+7 time zone
            //TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            //// Convert the UTC time to UTC+7
            //DateTime utcPlus7Now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            #endregion

            // Get the current UTC time and add a 7-hour offset
            DateTime utcPlus7Now = DateTime.UtcNow.AddHours(7);

            return utcPlus7Now;
        }

        /// <summary>
        /// Hàm này dùng để validate và kết hợp họ và tên thành một chuỗi duy nhất.
        /// </summary>
        /// <param name="firstName">Chuỗi chứa tên (firstName/givenName) cần được kết hợp.</param>
        /// <param name="lastName">Chuỗi chứa họ (lastName/surName) cần được kết hợp.</param>
        /// <returns>
        /// Chuỗi đã được kết hợp giữa họ và tên, với một khoảng trắng ở giữa.
        /// Nếu họ hoặc tên là chuỗi rỗng hoặc null, kết quả sẽ không có khoảng trắng thừa.
        /// </returns>
        public static string? ValidateAndCombineName(string? firstName, string? lastName)
        {
            // Nếu cả 2 biến đều là null thì trả về null thay vì chuỗi rỗng
            if(firstName == null && lastName == null)
            {
                return null;
            }

            // Cắt bỏ khoảng trắng ở đầu và cuối của firstName và lastName
            string trimmedFirstName = firstName?.Trim() ?? string.Empty;
            string trimmedLastName = lastName?.Trim() ?? string.Empty;

            // Kết hợp firstName và lastName với một khoảng trắng ở giữa
            string fullName = $"{trimmedLastName} {trimmedFirstName}".Trim();

            return fullName;
        }

        public static string? GetTitleCustomException(string? title, string baseTitle) => string.IsNullOrEmpty(title) ? baseTitle : title;

        public static async Task<Image> GetImageInfoFromUrl(string url)
        {
            using HttpClient client = new();

            // Tải dữ liệu ảnh từ URL
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Đọc nội dung dưới dạng byte
            var imageData = await response.Content.ReadAsByteArrayAsync();

            // Tạo đối tượng Image từ dữ liệu byte
            using var ms = new MemoryStream(imageData);

            // Sử dụng System.Drawing.Image
            var image = Image.FromStream(ms); 
            return image;
        }

        public static string EscapeSpecialCharacters(string input)
        {
            return Regex.Escape(input);  // Thoát toàn bộ ký tự đặc biệt trong Regular Expression
        }

        private static IHttpContextAccessor _httpContextAccessor;

        // Hàm này để truyền IHttpContextAccessor từ DI vào lớp static
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string GetIpAddress()
        {
            var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return ipAddress ?? "IP address not available";
        }

        public static  string GenerateOTP()
        {
            //using thay cho .Dispose, rng sẽ được giải phóng sau khi đóng using
            using var rng = RandomNumberGenerator.Create();
            var otpBytes = new byte[4];
            rng.GetBytes(otpBytes);
            //chuyển mảng byte sang int và chia lấy dư cho 1000000 để lấy đúng 6 chữ số sau dấu phẩy
            int otpCode = BitConverter.ToInt32(otpBytes, 0) % 1000000;
            return Math.Abs(otpCode).ToString("D6"); // chuyển thành chuỗi 6 chữ số
        }

        // Phương thức này lấy giá trị của EnumMemberAttribute từ một giá trị enum
        // Trả về string
        public static string GetEnumMemberValue<T>(T enumValue) where T : Enum
        {
            // Lấy thông tin trường (field) của giá trị enum
            FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());

            // Kiểm tra nếu trường không null
            if (field != null)
            {
                // Lấy thuộc tính EnumMemberAttribute từ trường
                EnumMemberAttribute? attribute = Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute)) as EnumMemberAttribute;

                // Nếu thuộc tính không null, trả về giá trị của thuộc tính
                if (attribute != null)
                {
                    return attribute.Value ?? enumValue.ToString();
                }
            }

            // Nếu không tìm thấy thuộc tính, trả về giá trị enum dưới dạng chuỗi
            return enumValue.ToString();
        }

        public static (int height, int width) GetImageDimensions(Stream? stream)
        {
            // Load the image from the stream
            using var image = Image.FromStream(stream);

            // Retrieve width and height
            int height = image.Height;
            int width = image.Width;
            
            return (height, width);
        }
    }
}
