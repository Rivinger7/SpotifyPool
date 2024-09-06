namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Tùy chỉnh exception
    /// </summary>
    public class CustomException : Exception
    {
        public string? Title { get; }
        public int? StatusCode { get; }

        /// <summary>
        /// Tùy chỉnh exception
        /// </summary>
        /// <param name="title"></param>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public CustomException(string title ,int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }
    }

}
