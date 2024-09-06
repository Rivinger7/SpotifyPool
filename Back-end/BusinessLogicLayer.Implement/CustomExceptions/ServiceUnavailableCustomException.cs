namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi một dịch vụ quan trọng không khả dụng (thường là vấn đề từ phía server hoặc hạ tầng)
    /// </summary>
    public class ServiceUnavailableCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public ServiceUnavailableCustomException() : base() { }
        public ServiceUnavailableCustomException(string message) : base(message) { }
        public ServiceUnavailableCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public ServiceUnavailableCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public ServiceUnavailableCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public ServiceUnavailableCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
