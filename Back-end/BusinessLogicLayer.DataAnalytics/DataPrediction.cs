using Microsoft.ML;
using Microsoft.ML.Data;

namespace BusinessLogicLayer.DataAnalytics
{
    public class DataPrediction : IDataPrediction
    {
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
