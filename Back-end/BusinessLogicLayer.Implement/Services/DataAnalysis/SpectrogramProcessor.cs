using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using Xabe.FFmpeg;
using BitmapDrawing = System.Drawing.Bitmap;

namespace BusinessLogicLayer.Implement.Services.DataAnalysis
{
    public static class SpectrogramProcessor
    {
        // Xác định hệ điều hành
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        //public static BitmapDrawing ConvertToSpectrogram(IFormFile audioFile)
        //{
        //    // Đọc audio từ URL hoặc file
        //    (double[] audioData, int sampleRateData) = ReadMono(audioFile);

        //    // Khởi tạo dữ liệu spectrogram
        //    int fftSize = 16384;
        //    int targetWidthPx = 3000;

        //    // Tính toán step size
        //    int stepSize = audioData.Length / targetWidthPx;

        //    // Khởi tạo SpectrogramGenerator
        //    Spectrogram.SpectrogramGenerator sg = new(sampleRateData, fftSize, stepSize, maxFreq: 2200);

        //    // Thêm dữ liệu audio vào spectrogram
        //    sg.Add(audioData);

        //    var bitmap = sg.GetBitmap(intensity: 5, dB: true);

        //    return bitmap;
        //}

        public static async Task<BitmapDrawing> ConvertToSpectrogram(string previewAudio)
        {
            // Đọc audio từ URL hoặc file
            (double[] audioData, int sampleRateData) = await ReadMono(previewAudio);

            // Khởi tạo dữ liệu spectrogram
            int fftSize = 16384;
            int targetWidthPx = 3000;

            // Tính toán step size
            int stepSize = audioData.Length / targetWidthPx;

            // Khởi tạo SpectrogramGenerator
            Spectrogram.SpectrogramGenerator sg = new(sampleRateData, fftSize, stepSize, maxFreq: 2200);

            // Thêm dữ liệu audio vào spectrogram
            sg.Add(audioData);

            var bitmap = sg.GetBitmap(intensity: 5, dB: true);

            return bitmap;
        }

        //private static (double[] audio, int sampleRate) ReadMono(IFormFile audioFile, double multiplier = 16_000)
        //{
        //    if (audioFile == null || audioFile.Length == 0)
        //    {
        //        throw new ArgumentException("File âm thanh không hợp lệ.");
        //    }

        //    using var stream = new MemoryStream();
        //    audioFile.CopyTo(stream);
        //    stream.Position = 0; // Reset lại vị trí đầu stream

        //    using var afr = new Mp3FileReader(stream); // Đọc MP3 trực tiếp từ MemoryStream
        //    using var pcmStream = WaveFormatConversionStream.CreatePcmStream(afr); // Chuyển đổi sang PCM
        //    using var waveProvider = new WaveChannel32(pcmStream); // Chuyển đổi sang float

        //    int sampleRate = waveProvider.WaveFormat.SampleRate;
        //    var audio = new List<double>();
        //    var buffer = new float[1024];

        //    var sampleProvider = waveProvider.ToSampleProvider(); // Chuyển sang SampleProvider để đọc float[]

        //    int samplesRead;
        //    while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
        //    {
        //        audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
        //    }

        //    return (audio.ToArray(), sampleRate);
        //}

        private static async Task<(double[] audio, int sampleRate)> ReadMono(string filePath, double multiplier = 16_000)
        {
            //using var afr = new NAudio.Wave.AudioFileReader(filePath);
            //int sampleRate = afr.WaveFormat.SampleRate;
            //int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            //int sampleCount = (int)(afr.Length / bytesPerSample);
            //int channelCount = afr.WaveFormat.Channels;
            //var audio = new List<double>(sampleCount);
            //var buffer = new float[sampleRate * channelCount];
            //int samplesRead = 0;
            //while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
            //    audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            //return (audio.ToArray(), sampleRate);

            string outputWavPath = Path.ChangeExtension(filePath, ".wav");

            // Chuyển file sang WAV bằng FFmpeg
            await FFmpeg.Conversions.New()
                .AddParameter($"-i \"{filePath}\" -ac 1 -ar 16000 \"{outputWavPath}\"")
                .Start();

            // Đọc dữ liệu WAV đã convert
            using var afr = new FileStream(outputWavPath, FileMode.Open, FileAccess.Read);
            var waveBytes = new byte[afr.Length];
            await afr.ReadAsync(waveBytes, 0, waveBytes.Length);

            // Giả lập đọc sampleRate, bạn có thể cải thiện bằng cách đọc header WAV
            int sampleRate = 16000;

            // Chuyển đổi byte thành float samples
            List<double> audio = [];
            for (int i = 44; i < waveBytes.Length; i += 2) // WAV header có 44 bytes
            {
                short sample = BitConverter.ToInt16(waveBytes, i);
                audio.Add(sample * multiplier / short.MaxValue);
            }

            return (audio.ToArray(), sampleRate);
        }

        public static async Task<Tensor<float>> ProcessImageToTensor(IFormFile imagePath, int targetWidth = 128, int targetHeight = 128)
        {
            using Stream imageStream = imagePath.OpenReadStream();

            // Đọc ảnh WebP
            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(imageStream);

            // Resize ảnh
            image.Mutate(x => x.Resize(targetWidth, targetHeight));

            // Trích xuất pixel
            float[] normalizedPixels = new float[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    Rgba32 pixel = image[x, y];
                    float grayscale = (pixel.R + pixel.G + pixel.B) / 3f / 255f;
                    normalizedPixels[y * targetWidth + x] = grayscale;
                }
            }

            // Tạo tensor
            return new DenseTensor<float>(normalizedPixels, [1, targetHeight, targetWidth, 1]);
        }

        public static async Task<Tensor<float>> ProcessImageToTensor(string imagePath, int targetWidth = 128, int targetHeight = 128)
        {
            using HttpClient client = new();
            using Stream imageStream = await client.GetStreamAsync(imagePath);

            // Đọc ảnh WebP
            using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(imageStream);

            // Resize ảnh
            image.Mutate(x => x.Resize(targetWidth, targetHeight));

            // Trích xuất pixel
            float[] normalizedPixels = new float[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    Rgba32 pixel = image[x, y];
                    float grayscale = (pixel.R + pixel.G + pixel.B) / 3f / 255f;
                    normalizedPixels[y * targetWidth + x] = grayscale;
                }
            }

            // Tạo tensor
            return new DenseTensor<float>(normalizedPixels, [1, targetHeight, targetWidth, 1]);
        }

        public static float[] Predict(Tensor<float> inputTensor)
        {
            // Đường dẫn tới mô hình ONNX
            string basePath;
            string onnxModelPath;

            if(IsWindows)
            {
                basePath = Directory.GetCurrentDirectory();
                onnxModelPath = Path.Combine(basePath, "TrainingModel", "audio_features_model.onnx");
            }
            else if (IsLinux)
            {
                basePath = "/app";
                onnxModelPath = Path.Combine(basePath, "audio_features_model.onnx");
            }
            else
            {
                throw new PlatformNotSupportedException("This platform is not supported");
            }

            // Chạy suy luận
            SessionOptions options = new();
            options.RegisterCustomOpLibraryV2("/usr/local/lib/onnxruntime/lib/libonnxruntime.so", out _);
            using InferenceSession session = new(onnxModelPath, options);
            string inputName = session.InputMetadata.Keys.First();
            List<NamedOnnxValue> inputs =
            [
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            ];

            // Lấy kết quả suy luận
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            return results.First().AsEnumerable<float>().ToArray();
        }
    }
}
