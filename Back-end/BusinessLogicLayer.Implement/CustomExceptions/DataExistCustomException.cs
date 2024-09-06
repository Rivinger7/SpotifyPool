namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi cố gắng tạo hoặc thêm dữ liệu đã tồn tại
    /// </summary>
    public class DataExistCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public DataExistCustomException() : base() { }
        public DataExistCustomException(string message) : base(message) { }
        public DataExistCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public DataExistCustomException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
        public DataExistCustomException(string title, int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }
        public DataExistCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
