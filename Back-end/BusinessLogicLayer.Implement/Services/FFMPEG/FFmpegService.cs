using BusinessLogicLayer.Interface.Microservices_Interface.AWS;
using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace BusinessLogicLayer.Implement.Services.FFMPEG
{
    public class FFmpegService : IFFmpegService
    {
        public FFmpegService(IAmazonWebService amazonWebService)
        {
            //_ = EnsureFFmpegExists(); // Đảm bảo FFmpeg đã có, nếu chưa có thì tải xuống
            EnsureFFmpegExists().Wait();
        }

        private async Task EnsureFFmpegExists()
        {
            // Đặt đường dẫn FFmpeg về thư mục chính của backend
            //string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string ffmpegPath = Directory.GetCurrentDirectory();

            //Console.WriteLine("==================================");
            //Console.WriteLine($"{Directory.GetCurrentDirectory()}");
            //Console.WriteLine($"{AppDomain.CurrentDomain.BaseDirectory}");
            //Console.WriteLine("==================================");

            // Chuẩn hóa đường dẫn
            ffmpegPath = Path.GetFullPath(ffmpegPath);

            // Thiết lập đường dẫn FFmpeg
            FFmpeg.SetExecutablesPath(ffmpegPath);

            bool ffmpegExists = File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) ||
                                File.Exists(Path.Combine(ffmpegPath, "ffmpeg"));

            if (!ffmpegExists)
            {
                Console.WriteLine("FFmpeg chưa tồn tại, đang tải xuống...");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
                Console.WriteLine($"FFmpeg đã tải xuống thành công tại: {ffmpegPath}");
            }
            else
            {
                //Console.WriteLine($"FFmpeg đã tồn tại ở {ffmpegPath}, bỏ qua tải xuống.");
            }
        }


        public async Task<string> ConvertToHls(IFormFile audioFile, string trackId)
        {
            string? inputFileTemp = null;
            string outputFilePath = string.Empty;
            string outputFolder = string.Empty;
            try
            {
                if (audioFile == null || audioFile.Length == 0)
                    throw new ArgumentException("File âm thanh không hợp lệ.");

                // Lấy đường dẫn gốc của dự án (Back-end)
                //string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string basePath = Directory.GetCurrentDirectory();

                // Định nghĩa đường dẫn tương đối từ thư mục Back-end
                string inputFolder = Path.Combine(basePath, "Commons", "temp", "input_audio");
                outputFolder = Path.Combine(basePath, "Commons", "temp", "output_audio", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(inputFolder))
                    Directory.CreateDirectory(inputFolder);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                // Tạo tên file input tạm
                inputFileTemp = Path.Combine(inputFolder, ObjectId.GenerateNewId().ToString() + $"{Path.GetExtension(audioFile.FileName)}");

                // Lưu IFormFile thành file tạm
                using (FileStream fileStream = new(inputFileTemp, FileMode.Create))
                {
                    await audioFile.CopyToAsync(fileStream);
                }

                outputFilePath = Path.Combine(outputFolder, $"{trackId}_output.m3u8");

                // Kiểm tra file đầu vào có hợp lệ không
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFileTemp);
                if (!mediaInfo.AudioStreams.Any())
                    throw new InvalidOperationException("File không chứa stream âm thanh hợp lệ.");

                // Chuyển đổi bằng cách thêm Stream thay vì AddParameter
                IConversion conversion = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.AudioStreams.FirstOrDefault()) // Lấy stream âm thanh
                    .SetOutput(outputFilePath)
                    .AddParameter("-c:a aac -b:a 128k -hls_time 10 -hls_playlist_type vod");

                await conversion.Start();
            }
            finally
            {
                // Xóa file input sau khi xử lý xong để tránh rác
                if (!string.IsNullOrEmpty(inputFileTemp) && File.Exists(inputFileTemp))
                    File.Delete(inputFileTemp);
            }

            return outputFolder;
        }
    }
}
