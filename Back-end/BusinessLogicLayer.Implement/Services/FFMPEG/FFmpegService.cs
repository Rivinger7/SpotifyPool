using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Utility.Coding;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using Path = System.IO.Path;

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

        private void DeleteOldFFmpegFiles(string directory)
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


        // Convert IFormFile to Waveform Audio File
        public async Task<(string, long?)> ConvertToWavFileAsync(IFormFile inputFile, string? basePath, string? rootFolder, string? inputIntermediateFolder, string? ouputIntermediateFolder)
        {
            if (inputFile == null || inputFile.Length == 0)
                throw new ArgumentException("Tệp âm thanh không hợp lệ.");

            // Tạo file tạm input (mp3, m4a...)
            string inputFileExtension = Path.GetExtension(inputFile.FileName);

            //string inputTempPath = Path.Combine(Path.GetTempPath(), $"{ObjectId.GenerateNewId()}{inputFileExtension}");

            basePath ??= Path.GetTempPath(); // Nếu basePath null thì dùng thư mục tạm hệ thống
            rootFolder ??= string.Empty;
            inputIntermediateFolder ??= string.Empty;
            ouputIntermediateFolder ??= string.Empty;
            string inputFileName = Path.GetFileNameWithoutExtension(inputFile.FileName);

            // Nếu basePath, rootFolder, intermediateFolder không null thì tạo đường dẫn tạm theo cấu trúc
            string inputFolderTempPath = Path.Combine(basePath, rootFolder, inputIntermediateFolder);
            string outputFolderTempPath = Path.Combine(basePath, rootFolder, ouputIntermediateFolder);

            string outputWavPath = string.Empty;
            long? bitrate = null;

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(inputFolderTempPath))
            {
                Directory.CreateDirectory(inputFolderTempPath);
            }
            if (!Directory.Exists(outputFolderTempPath))
            {
                Directory.CreateDirectory(outputFolderTempPath);
            }

            try
            {
                string inputTempPath = Path.Combine(inputFolderTempPath, $"{ObjectId.GenerateNewId()}_{inputFileName}{inputFileExtension}");
                using (var stream = new FileStream(inputTempPath, FileMode.Create))
                {
                    await inputFile.CopyToAsync(stream);
                }

                // Tạo đường dẫn file .wav tạm
                outputWavPath = Path.Combine(outputFolderTempPath, $"{ObjectId.GenerateNewId()}.wav");

                // Kiểm tra file đầu vào có hợp lệ không
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputTempPath);
                if (!mediaInfo.AudioStreams.Any())
                    throw new InvalidOperationException("Tệp âm thanh không chứa stream âm thanh hợp lệ.");

                // Lấy stream âm thanh đầu tiên (nếu có nhiều stream thì lấy stream đầu tiên)
                IAudioStream? audioStream = mediaInfo.AudioStreams.FirstOrDefault();

                // Nếu không có bitrate thì dùng 128k
                bitrate = audioStream?.Bitrate ?? 128000;

                // Convert dùng Xabe.FFmpeg
                //IConversion conversion = await FFmpeg.Conversions.FromSnippet.Convert(inputTempPath, outputWavPath);
                //conversion.AddParameter("-ac 1 -ar 16000"); // Mono, 16kHz nếu cần
                IConversion conversion = FFmpeg.Conversions.New()
                    .AddStream(audioStream)
                    .SetOutput(outputWavPath);

                await conversion.Start();

                // Xoá input tạm
                if (File.Exists(inputTempPath))
                {
                    File.Delete(inputTempPath);
                }
            }
            catch
            {
                // Xoá thư mục tạm nếu có lỗi xảy ra
                if (Directory.Exists(inputFolderTempPath))
                {
                    Directory.Delete(inputFolderTempPath, true); // Xóa cả file bên trong
                }
                if (Directory.Exists(outputFolderTempPath))
                {
                    Directory.Delete(outputFolderTempPath, true); // Xóa cả file bên trong
                }
            }

            return (outputWavPath, bitrate);
        }

        public void DeleteFileAsync(string filePath)
        {
            File.Delete(filePath);
            return;
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
                    throw new ArgumentException("AudioFile âm thanh không hợp lệ.");

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
                    throw new InvalidOperationException("AudioFile không chứa stream âm thanh hợp lệ.");

                // Dùng bitrate gốc nếu có, hoặc fallback về 128k hoặc 256k
                long bitrate = mediaInfo.AudioStreams.FirstOrDefault().Bitrate;

                // Chuyển đổi bằng cách thêm Stream thay vì AddParameter
                IConversion conversion = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.AudioStreams.FirstOrDefault()) // Lấy stream âm thanh
                    .SetOutput(outputFilePath)
                    .AddParameter($"-c:a aac -b:a {bitrate} -hls_time 10 -hls_playlist_type vod");

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
                //if (!string.IsNullOrEmpty(inputFileTemp) && AudioFile.Exists(inputFileTemp))
                //    AudioFile.Delete(inputFileTemp);

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


        public async Task<string> ConvertToHlsTemp(string audioFilePath, string trackId, string? basePath, string? rootFolder, string? outputIntermediateFolder, string? targetFolder = null)
        {
            string outputFolder = string.Empty;
            string outputFilePath = string.Empty;

            basePath ??= Path.GetTempPath(); // Nếu basePath null thì dùng thư mục tạm hệ thống
            rootFolder ??= string.Empty;
            outputIntermediateFolder ??= string.Empty;
            targetFolder ??= ObjectId.GenerateNewId().ToString();

            try
            {
                outputFolder = Path.Combine(basePath, rootFolder, outputIntermediateFolder, $"{targetFolder}");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                // Cấp quyền cho thư mục
                //Syscall.chmod(inputFolder, FilePermissions.ALLPERMS);
                //Syscall.chmod(outputFolder, FilePermissions.ALLPERMS);

                outputFilePath = Path.Combine(outputFolder, $"{trackId}_hls.m3u8");

                // Kiểm tra file đầu vào có hợp lệ không
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(audioFilePath);
                if (!mediaInfo.AudioStreams.Any())
                    throw new InvalidOperationException("AudioFile không chứa stream âm thanh hợp lệ.");

                long bitrate = mediaInfo.AudioStreams?.FirstOrDefault().Bitrate ?? 128000;

                // Chuyển đổi bằng cách thêm Stream thay vì AddParameter
                IConversion conversion = FFmpeg.Conversions.New()
                    .AddStream(mediaInfo.AudioStreams.FirstOrDefault()) // Lấy stream âm thanh
                    .SetOutput(outputFilePath)
                    .AddParameter($"-c:a aac -b:a {bitrate} -hls_time 10 -hls_playlist_type vod");

                await conversion.Start();
            }
            catch
            {
                if (Directory.Exists(audioFilePath))
                {
                    Directory.Delete(audioFilePath, true); // Xóa cả file bên trong
                }

                if (Directory.Exists(outputFolder))
                {
                    Directory.Delete(outputFolder, true); // Xóa cả file bên trong
                }
            }

            return outputFilePath;
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
