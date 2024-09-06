namespace BusinessLogicLayer.Implement.CustomExceptions
{
    public class BadRequestCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public BadRequestCustomException() : base() { }
        public BadRequestCustomException(string message) : base(message) { }
        public BadRequestCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public BadRequestCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public BadRequestCustomException(string title, int statuscode, string message) : base(message)
        {
            Title = title;
            StatusCode = statuscode;
        }
        public BadRequestCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
