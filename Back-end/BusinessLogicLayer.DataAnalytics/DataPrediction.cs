using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace BusinessLogicLayer.DataAnalytics
{
    public class DataPrediction : IDataPrediction
    {
        private static readonly string _modelFilePath = "model.zip";
        private readonly MLContext _mlContext;

        public DataPrediction()
        {
            _mlContext = new MLContext();
        }

        public void TrainModel()
        {
            var ratings = new List<UserRating>
    {
        new UserRating { UserId = 1, ProductId = 1, Rating = 5 },
        new UserRating { UserId = 1, ProductId = 2, Rating = 3 },
        new UserRating { UserId = 2, ProductId = 1, Rating = 4 },
        new UserRating { UserId = 2, ProductId = 3, Rating = 2 },
        new UserRating { UserId = 3, ProductId = 2, Rating = 5 },
        new UserRating { UserId = 3, ProductId = 3, Rating = 4 }
    };

            // Chuyển dữ liệu thành IDataView
            IDataView trainData = _mlContext.Data.LoadFromEnumerable(ratings);

            // Huấn luyện mô hình sử dụng Matrix Factorization với KeyType cho cột chỉ số ma trận
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(UserRating.UserId), // Cột người dùng
                MatrixRowIndexColumnName = nameof(UserRating.ProductId), // Cột sản phẩm
                LabelColumnName = nameof(UserRating.Rating),             // Cột rating
                NumberOfIterations = 20,
                Alpha = 0.1f
            };

            var trainer = _mlContext.Recommendation().Trainers.MatrixFactorization(options);

            var model = trainer.Fit(trainData);

            // Lưu mô hình đã huấn luyện vào file
            _mlContext.Model.Save(model, trainData.Schema, _modelFilePath);
        }

        public float Predict(UserPredictionRequest input)
        {
            /// Nếu mô hình chưa được huấn luyện, huấn luyện lại
            if (!File.Exists(_modelFilePath))
            {
                TrainModel();
            }

            // Tải mô hình đã huấn luyện
            var model = _mlContext.Model.Load(_modelFilePath, out var modelInputSchema);

            // In thông tin schema của mô hình
            foreach (var column in modelInputSchema)
            {
                Console.WriteLine($"Column Name: {column.Name}, Type: {column.Type}");
            }

            // Tạo prediction engine để dự đoán
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<UserPredictionRequest, ProductPrediction>(model);

            // Dự đoán điểm số cho sản phẩm của người dùng
            ProductPrediction prediction = predictionEngine.Predict(input);

            return prediction.PredictedRating;  // Trả về điểm số dự đoán
        }

        //public static IEnumerable<AudioFeaturesResponseModel> LoadDataFromFile(IFormFile file)
        //{
        //    var mlContext = new MLContext();
        //    List<AudioFeaturesResponseModel> songs = [];

        //    // Tạo một tệp tạm thời
        //    string tempFilePath = Path.GetTempFileName();

        //    try
        //    {
        //        // Sao chép nội dung của IFormFile vào tệp tạm thời
        //        using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
        //        {
        //            file.CopyTo(fileStream);
        //        }

        //        // Sử dụng TextLoader để đọc tệp từ đường dẫn
        //        var textLoader = mlContext.Data.CreateTextLoader(new TextLoader.Options
        //        {
        //            Separators = [','],
        //            HasHeader = true,
        //            Columns =
        //            [
        //        new TextLoader.Column("Id", DataKind.String, 0),
        //        new TextLoader.Column("Duration", DataKind.Single, 1),
        //        new TextLoader.Column("Key", DataKind.Single, 2),
        //        new TextLoader.Column("TimeSignature", DataKind.Single, 3),
        //        new TextLoader.Column("Mode", DataKind.Single, 4),
        //        new TextLoader.Column("Acousticness", DataKind.Single, 5),
        //        new TextLoader.Column("Danceability", DataKind.Single, 6),
        //        new TextLoader.Column("Energy", DataKind.Single, 7),
        //        new TextLoader.Column("Instrumentalness", DataKind.Single, 8),
        //        new TextLoader.Column("Liveness", DataKind.Single, 9),
        //        new TextLoader.Column("Loudness", DataKind.Single, 10),
        //        new TextLoader.Column("Speechiness", DataKind.Single, 11),
        //        new TextLoader.Column("Tempo", DataKind.Single, 12),
        //        new TextLoader.Column("Valence", DataKind.Single, 13)
        //    ]
        //        });

        //        // Tạo IDataView từ tệp CSV
        //        var dataView = textLoader.Load(tempFilePath);

        //        // Chuyển đổi IDataView thành List<MusicData>
        //        songs = mlContext.Data.CreateEnumerable<AudioFeaturesResponseModel>(dataView, reuseRowObject: false).ToList();
        //    }
        //    finally
        //    {
        //        // Xóa tệp tạm thời sau khi đọc xong
        //        if (File.Exists(tempFilePath))
        //        {
        //            File.Delete(tempFilePath);
        //        }
        //    }

        //    return songs;
        //}
    }
}
