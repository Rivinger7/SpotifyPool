using System.Globalization;
using System.Text;

namespace Shared.Helpers
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
    }
}
