namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi dữ liệu cần thiết không được tìm thấy trong cơ sở dữ liệu hoặc nguồn dữ liệu khác
    /// </summary>
    public class DataNotFoundCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public DataNotFoundCustomException() : base() { }
        public DataNotFoundCustomException(string message) : base(message) { }
        public DataNotFoundCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public DataNotFoundCustomException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
        public DataNotFoundCustomException(string title, int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }

        public DataNotFoundCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
