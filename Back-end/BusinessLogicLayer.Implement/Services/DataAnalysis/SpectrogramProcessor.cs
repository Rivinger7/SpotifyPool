using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
//using Spectrogram;
using BitmapDrawing = System.Drawing.Bitmap;
using SixLabors.ImageSharp.Processing;

namespace BusinessLogicLayer.Implement.Services.DataAnalysis
{
    public static class SpectrogramProcessor
    {
        public static BitmapDrawing ConvertToSpectrogram(string previewAudio)
        {
            // Đọc audio từ URL hoặc file
            (double[] audioData, int sampleRateData) = ReadMono(previewAudio);

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

        private static (double[] audio, int sampleRate) ReadMono(string filePath, double multiplier = 16_000)
        {
            using var afr = new NAudio.Wave.AudioFileReader(filePath);
            int sampleRate = afr.WaveFormat.SampleRate;
            int bytesPerSample = afr.WaveFormat.BitsPerSample / 8;
            int sampleCount = (int)(afr.Length / bytesPerSample);
            int channelCount = afr.WaveFormat.Channels;
            var audio = new List<double>(sampleCount);
            var buffer = new float[sampleRate * channelCount];
            int samplesRead = 0;
            while ((samplesRead = afr.Read(buffer, 0, buffer.Length)) > 0)
                audio.AddRange(buffer.Take(samplesRead).Select(x => x * multiplier));
            return (audio.ToArray(), sampleRate);
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
            string currentDirectory = Directory.GetCurrentDirectory();
            string onnxModelPath = Path.Combine(currentDirectory, "4. Application", "audio_features_model.onnx");

            // Chạy suy luận
            using InferenceSession session = new(onnxModelPath);
            var inputName = session.InputMetadata.Keys.First();
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
