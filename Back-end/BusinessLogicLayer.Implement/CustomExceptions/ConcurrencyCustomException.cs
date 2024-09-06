namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi có xung đột trong việc truy cập dữ liệu đồng thời
    /// </summary>
    public class ConcurrencyCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public ConcurrencyCustomException() : base() { }
        public ConcurrencyCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public ConcurrencyCustomException(string message) : base(message) { }
        public ConcurrencyCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public ConcurrencyCustomException(string title ,int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public ConcurrencyCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
