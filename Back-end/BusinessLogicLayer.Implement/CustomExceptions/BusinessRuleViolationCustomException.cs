namespace BusinessLogicLayer.Implement.CustomExceptions
{
    /// <summary>
    /// Xảy ra khi một quy tắc nghiệp vụ bị vi phạm
    /// </summary>
    public class BusinessRuleViolationCustomException : Exception
    {
        public string? Title { get; } = string.Empty;
        public int? StatusCode { get; }
        public BusinessRuleViolationCustomException() : base() { }
        public BusinessRuleViolationCustomException(string message) : base(message) { }
        public BusinessRuleViolationCustomException(string title, string message) : base(message)
        {
            Title = title;
        }
        public BusinessRuleViolationCustomException(int statuscode, string message) : base(message)
        {
            StatusCode = statuscode;
        }
        public BusinessRuleViolationCustomException(string title, int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }

        public BusinessRuleViolationCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
