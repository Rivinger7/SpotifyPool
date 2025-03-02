using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System.Runtime.InteropServices;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace BusinessLogicLayer.Implement.Services.FFMPEG
{
    public class FFmpegService : IFFmpegService
    {
        // Xác định hệ điều hành
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public FFmpegService()
        {
            //_ = EnsureFFmpegExists(); // Đảm bảo FFmpeg đã có, nếu chưa có thì tải xuống
            EnsureFFmpegExists().Wait();
        }

        private static async Task EnsureFFmpegExists()
        {
            // Đặt đường dẫn FFmpeg về thư mục chính của backend
            //string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string ffmpegPath = Directory.GetCurrentDirectory();

            #region
            //Console.WriteLine("==================================");
            //Console.WriteLine($"{Directory.GetCurrentDirectory()}");
            //Console.WriteLine($"{AppDomain.CurrentDomain.BaseDirectory}");
            //Console.WriteLine("==================================");

            //string directoryPath = Directory.GetCurrentDirectory();

            //if (Directory.Exists(directoryPath))
            //{
            //    string[] directories = Directory.GetDirectories(directoryPath);

            //    Console.WriteLine($"📂 Danh sách thư mục trong {directoryPath}:");
            //    foreach (var dir in directories)
            //    {
            //        Console.WriteLine($"- {dir}");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"⚠️ Thư mục {directoryPath} không tồn tại!");
            //}

            //string tmpPath = "/tmp";

            //if (Directory.Exists(tmpPath))
            //{
            //    string[] directories = Directory.GetDirectories(tmpPath);
            //    Console.WriteLine($"📂 Danh sách thư mục trong {tmpPath}:");
            //    foreach (var dir in directories)
            //    {
            //        Console.WriteLine($"- {dir}");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"⚠️ Thư mục {tmpPath} không tồn tại!");
            //}

            //string spotifyPoolPath = Path.Combine(Directory.GetCurrentDirectory(), "Back-end", "SpotifyPool");

            //if (Directory.Exists(spotifyPoolPath))
            //{
            //    Console.WriteLine($"Thư mục {spotifyPoolPath} tồn tại.");
            //}
            //else
            //{
            //    Console.WriteLine($"Thư mục {spotifyPoolPath} không tồn tại.");
            //}

            //string backendPath = Path.Combine(Directory.GetCurrentDirectory(), "Back-end");

            //if (Directory.Exists(backendPath))
            //{
            //    Console.WriteLine($"Thư mục {backendPath} tồn tại.");
            //}
            //else
            //{
            //    Console.WriteLine($"Thư mục {backendPath} không tồn tại.");
            //}
            #endregion

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


        public async Task<(string, string, string)> ConvertToHls(IFormFile audioFile, string trackId)
        {
            string inputFolder = string.Empty;
            string inputFileTemp = string.Empty;
            string outputFilePath = string.Empty;
            string outputFolder = string.Empty;
            try
            {
                if (audioFile == null || audioFile.Length == 0)
                    throw new ArgumentException("File âm thanh không hợp lệ.");

                // Lấy đường dẫn gốc của dự án (Back-end)
                //string basePath = AppDomain.CurrentDomain.BaseDirectory;
                //string basePath = Directory.GetCurrentDirectory();

                //// Định nghĩa đường dẫn tương đối từ thư mục Back-end
                //string inputFolder = Path.Combine(basePath, "Commons", "temp", "input_audio");
                //outputFolder = Path.Combine(basePath, "Commons", "temp", "output_audio", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");

                //string basePath = "/tmp"; // Chỉ thư mục này có quyền ghi trên Render
                string basePath = string.Empty;
                

                if (IsWindows)
                {
                    basePath = AppDomain.CurrentDomain.BaseDirectory;

                    inputFolder = Path.Combine(basePath, "Commons", "input_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                    outputFolder = Path.Combine(basePath, "Commons", "output_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                }
                else if (IsLinux)
                {
                    basePath = "/var/data";

                    inputFolder = Path.Combine(basePath, "input_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                    outputFolder = Path.Combine(basePath, "output_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                }
                else
                {
                    throw new PlatformNotSupportedException("This platform is not supported");
                }

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(inputFolder))
                    Directory.CreateDirectory(inputFolder);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                // Cấp quyền cho thư mục
                //Syscall.chmod(inputFolder, FilePermissions.ALLPERMS);
                //Syscall.chmod(outputFolder, FilePermissions.ALLPERMS);

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
            catch
            {
                if (Directory.Exists(inputFolder))
                {
                    Directory.Delete(inputFolder, true); // Xóa cả file bên trong
                }

                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true); // Xóa cả file bên trong
                }
            }
            finally
            {
                // Xóa file input sau khi xử lý xong để tránh rác
                //if (!string.IsNullOrEmpty(inputFileTemp) && File.Exists(inputFileTemp))
                //    File.Delete(inputFileTemp);

                    //if (Directory.Exists(inputFolder))
                    //{
                    //    Directory.Delete(inputFolder, true); // Xóa cả file bên trong
                    //}

                    //if (Directory.Exists(outputFolder))
                    //{
                    //    Directory.Delete(outputFolder, true); // Xóa cả file bên trong
                    //}
            }

            return (inputFileTemp, inputFolder, outputFolder);
        }
    }
}
