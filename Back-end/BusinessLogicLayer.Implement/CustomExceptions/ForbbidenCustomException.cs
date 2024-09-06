namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi người dùng có xác thực nhưng không có quyền truy cập tài nguyên cụ thể
    /// </summary>
    public class ForbbidenCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public ForbbidenCustomException() : base() { }
        public ForbbidenCustomException(string message) : base(message) { }
        public ForbbidenCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public ForbbidenCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public ForbbidenCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public ForbbidenCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
