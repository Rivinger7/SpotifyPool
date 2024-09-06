namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi dữ liệu nhập vào hoặc dữ liệu lấy được từ nguồn không hợp lệ
    /// </summary>
    public class InvalidDataCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public InvalidDataCustomException() : base() { }
        public InvalidDataCustomException(string message) : base(message) { }
        public InvalidDataCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public InvalidDataCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public InvalidDataCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public InvalidDataCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
