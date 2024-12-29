//using MongoDB.Driver;
using Spectrogram;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BusinessLogicLayer.DataAnalytics.Spectrogram
{
    public static class SpectrogramAudio
    {
        #region Test
        //public void ConvertToSpectrogram()
        //{
        //    const string FILE_PATH = "C:/Users/Admins/Videos/12.mp3";
        //    const string FILE_URL_PATH = "https://p.scdn.co/mp3-preview/0f85ec265818a5cafa3318032fbdab036802feea?cid=a010b5500f8b45baa15ba73cf293d766";
        //    const string FILE_YT_PATH = "https://www.youtube.com/watch?v=kVXFwXekPfs";
        //    const string FILE_SAVE_PATH = "C:/Users/Admins/Pictures/firstspectrogram.png";
        //    const string FILE_SAVE_PATH_2 = "C:/Users/Admins/Pictures/secondspectrogram.png";
        //    const string FILE_SAVE_PATH_3 = "C:/Users/Admins/Pictures/thirdspectrogram.png";

        //    const string FOLDER_SAVE_PATH = @"Z:\SpotifyPool Project\Images\Results";

        //    // Projection
        //    ProjectionDefinition<Track, Track> trackProjection = Builders<Track>.Projection
        //        .Include(track => track.PreviewURL)
        //        .Include(track => track.AudioFeaturesId);

        //    // Lấy audio từ URL hoặc file
        //    IEnumerable<Track> tracks = _unitOfWork.GetCollection<Track>()
        //        .Find(track => true)
        //        .Project(trackProjection)
        //        .Limit(500)
        //        .ToList();

        //    // Khởi tạo dữ liệu spectrogram
        //    int fftSize = 16384;
        //    int targetWidthPx = 3000;
        //    int stepSize;
        //    SpectrogramGenerator sg;

        //    foreach (Track track in tracks)
        //    {
        //        if (string.IsNullOrEmpty(track.PreviewURL))
        //        {
        //            continue;
        //        }

        //        (double[] audioData, int sampleRateData) = ReadMono(track.PreviewURL);

        //        stepSize = audioData.Length / targetWidthPx;
        //        sg = new SpectrogramGenerator(sampleRateData, fftSize, stepSize, maxFreq: 2200);

        //        // Đặt tên file spectrogram theo AudioFeaturesId
        //        string fileName = $@"\{track.AudioFeaturesId}.png";

        //        sg.Add(audioData);

        //        // Chuyển đổi FFTs thành mảng hai chiều
        //        List<double[]> ffts = sg.GetFFTs();
        //        double[,] spectrogramData = ConvertFFTsToIntensity(ffts);

        //        // Chuyển dữ liệu spectrogram thành tensor
        //        Tensor<float> inputTensor = ProcessImageToTensor(spectrogramData);

        //        sg.SaveImage(FOLDER_SAVE_PATH + fileName, intensity: 5, dB: true);
        //    }

        //    // Convert audio file to spectrogram
        //}
        #endregion

        public static Tensor<float> ConvertToSpectrogram(string previewAudio)
        {
            // Đường dẫn tới thư mục trong gốc dự án
            string currentDirectory = Directory.GetCurrentDirectory();

            // Tìm thư mục SpectrogramData từ vị trí hiện tại
            string spectrogramFolder = Path.Combine(currentDirectory, "SpectrogramTempData");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(spectrogramFolder))
            {
                Directory.CreateDirectory(spectrogramFolder);
            }

            // Normalize đường dẫn (nếu cần thiết)
            //spectrogramFolder = Path.GetFullPath(spectrogramFolder);

            // Đặt tên file spectrogram theo Guid
            string fileName = $@"{Guid.NewGuid}.png";

            // Đường dẫn đến file spectrogram
            string tempFilePath = spectrogramFolder + '/' + fileName;

            Console.WriteLine(tempFilePath);

            // Khởi tạo tensor
            Tensor<float> inputTensor;

            // Quy trình xử lý ảnh spectrogram và chuyển thành tensor
            try
            {
                // Đọc audio từ URL hoặc file
                (double[] audioData, int sampleRateData) = ReadMono(previewAudio);

                // Khởi tạo dữ liệu spectrogram
                int fftSize = 16384;
                int targetWidthPx = 3000;

                // Tính toán step size
                int stepSize = audioData.Length / targetWidthPx;

                // Khởi tạo SpectrogramGenerator
                SpectrogramGenerator sg = new(sampleRateData, fftSize, stepSize, maxFreq: 2200);

                // Thêm dữ liệu audio vào spectrogram
                sg.Add(audioData);

                // Lưu ảnh spectrogram
                sg.SaveImage(tempFilePath, intensity: 5, dB: true);
                // Đọc ảnh spectrogram và chuyển thành tensor
                inputTensor = ProcessImageToTensor(tempFilePath);
            }
            finally
            {
                // Xóa file spectrogram sau khi sử dụng
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return inputTensor;
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

        // Hàm xử lý ảnh spectrogram thành tensor
        private static Tensor<float> ProcessImageToTensor(string imagePath, int targetWidth = 128, int targetHeight = 128)
        {
            // Đọc ảnh grayscale với OpenCV
            Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
            if (image.Empty())
            {
                throw new ArgumentException($"Image not found at path: {imagePath}");
            }

            // Resize ảnh về kích thước targetWidth x targetHeight
            Mat resizedImage = new();
            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(targetWidth, targetHeight), 0, 0, InterpolationFlags.Area);

            // Chuẩn hóa pixel
            float[] normalizedPixels = new float[targetWidth * targetHeight];
            for (int y = 0; y < resizedImage.Rows; y++)
            {
                for (int x = 0; x < resizedImage.Cols; x++)
                {
                    normalizedPixels[y * targetWidth + x] = resizedImage.At<byte>(y, x) / 255.0f; // Chuẩn hóa giá trị pixel
                }
            }

            // Tạo tensor với định dạng [1, targetHeight, targetWidth, 1]
            return new DenseTensor<float>(normalizedPixels, [1, targetHeight, targetWidth, 1]);
        }

        // Hàm chạy suy luận với mô hình ONNX
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
