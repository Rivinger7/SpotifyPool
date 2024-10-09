namespace BusinessLogicLayer.Implement.CustomExceptions
{
    public class ArgumentNullCustomException : ArgumentNullException
    {
        public int? StatusCode { get; }
        public ArgumentNullCustomException() : base() { }
        public ArgumentNullCustomException(string paramName) : base(paramName) { }
        public ArgumentNullCustomException(string paramName, string message) : base(paramName, message) { }
        public ArgumentNullCustomException(int statuscode, string paramName) : base(paramName)
        {
            StatusCode = statuscode;
        }
        public ArgumentNullCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
