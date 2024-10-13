using System.Globalization;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Http;

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
            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Define the UTC+7 time zone
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Convert the UTC time to UTC+7
            DateTime utcPlus7Now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);

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
    }
}
