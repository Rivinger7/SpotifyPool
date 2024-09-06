namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi có lỗi chưa xác định trong hệ thống
    /// </summary>
    public class InternalServerErrorCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public InternalServerErrorCustomException() : base() { }
        public InternalServerErrorCustomException(string message) : base(message) { }
        public InternalServerErrorCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public InternalServerErrorCustomException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
        public InternalServerErrorCustomException(string title, int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }
        public InternalServerErrorCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
