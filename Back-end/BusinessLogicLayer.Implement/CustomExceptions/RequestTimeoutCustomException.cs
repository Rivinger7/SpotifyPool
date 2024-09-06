namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi không nhận được yêu cầu hoàn chỉnh từ client trong một khoảng thời gian cho phép
    /// </summary>
    public class RequestTimeoutCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public RequestTimeoutCustomException() : base() { }
        public RequestTimeoutCustomException(string message) : base(message) { }
        public RequestTimeoutCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public RequestTimeoutCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public RequestTimeoutCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public RequestTimeoutCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
