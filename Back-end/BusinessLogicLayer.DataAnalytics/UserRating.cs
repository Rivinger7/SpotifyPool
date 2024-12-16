using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DataAnalytics
{
    public class UserRating
    {
        [KeyType(count: 3)] // Định nghĩa loại key cho các chỉ số UserId và ProductId
        public uint UserId { get; set; }

        [KeyType(count: 3)] // Định nghĩa loại key cho các chỉ số ProductId
        public uint ProductId { get; set; }
        public float Rating { get; set; }  // Xếp hạng vẫn giữ kiểu float
    }

    public class ProductPrediction
    {
        [KeyType(count: 3)] // Định nghĩa loại key cho các chỉ số UserId và ProductId
        public uint ProductId { get; set; }
        public float PredictedRating { get; set; }
    }

    public class UserPredictionRequest
    {
        [KeyType(count: 3)] // Định nghĩa loại key cho các chỉ số UserId và ProductId
        public uint UserId { get; set; }
        [KeyType(count: 3)] // Định nghĩa loại key cho các chỉ số UserId và ProductId
        public uint ProductId { get; set; }
    }
}
