using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Utility.Coding;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace BusinessLogicLayer.Implement.Services.FFMPEG
{
    public class FFmpegService : IFFmpegService
    {
        private bool isFFmpegChecked = false;
        private bool disposedValue;
        private static readonly SemaphoreSlim semaphore = new(1, 1); // Tránh chạy nhiều lần

        public FFmpegService()
        {
            EnsureFFmpegExists().GetAwaiter().GetResult();
        }

        private async Task EnsureFFmpegExists()
        {
            if (isFFmpegChecked)
            {
                return;
            }

            await semaphore.WaitAsync();

            // Đặt đường dẫn FFmpeg về thư mục chính của backend
            //string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");

            //string basePath;
            //string ffmpegFolder;
            string ffmpegPath;

            // Chuẩn hóa đường dẫn
            //ffmpegPath = Path.GetFullPath(ffmpegFolder);
            ffmpegPath = Path.GetFullPath(Directory.GetCurrentDirectory());

            // Thiết lập đường dẫn FFmpeg
            FFmpeg.SetExecutablesPath(ffmpegPath);

            bool ffmpegExists = File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) ||
                                File.Exists(Path.Combine(ffmpegPath, "ffmpeg"));

            try
            {
                if (!ffmpegExists)
                {
                    Console.WriteLine("FFmpeg chưa tồn tại, đang tải xuống...");

                    // Xóa phiên bản cũ trước khi tải
                    DeleteOldFFmpegFiles(ffmpegPath);

                    // Tải FFmpeg mới nhất
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
                    Console.WriteLine($"FFmpeg đã tải xuống thành công tại: {ffmpegPath}");

                    isFFmpegChecked = true;
                }
                else
                {
                    //Console.WriteLine($"FFmpeg đã tồn tại ở {ffmpegPath}, bỏ qua tải xuống.");
                }
            }
            finally
            {
                semaphore.Release(); // Giải phóng semaphore để lần sau có thể chạy tiếp
            }
        }

        private static void DeleteOldFFmpegFiles(string directory)
        {
            string[] oldFiles = Directory.GetFiles(directory, "ffmpeg*"); // Xóa ffmpeg.exe, ffmpeg-linux, ffmpeg-macos...
            string[] oldProbes = Directory.GetFiles(directory, "ffprobe*"); // Xóa ffprobe.exe nếu có

            foreach (var file in oldFiles.Concat(oldProbes))
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Đã xóa file cũ: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xóa file {file}: {ex.Message}");
                }
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

                // Tạo thư mục chứa file input và output
                string basePath = string.Empty;

                if (Util.IsWindows())
                {
                    basePath = AppDomain.CurrentDomain.BaseDirectory;

                    inputFolder = Path.Combine(basePath, "Commons", "input_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                    outputFolder = Path.Combine(basePath, "Commons", "output_temp_audio_hls", $"{ObjectId.GenerateNewId()}_{Path.GetFileNameWithoutExtension(audioFile.FileName)}");
                }
                else if (Util.IsLinux())
                {
                    //basePath = "/var/data";
                    basePath = "/tmp";

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FFmpegService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
