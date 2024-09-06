namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi người dùng không có quyền thực hiện một thao tác cụ thể
    /// </summary>
    public class UnAuthorizedCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public UnAuthorizedCustomException() : base() { }
        public UnAuthorizedCustomException(string message) : base(message) { }
        public UnAuthorizedCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public UnAuthorizedCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public UnAuthorizedCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public UnAuthorizedCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
