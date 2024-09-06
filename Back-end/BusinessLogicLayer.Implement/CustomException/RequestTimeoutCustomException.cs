using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi không nhận được yêu cầu hoàn chỉnh từ client trong một khoảng thời gian cho phép
    /// </summary>
    public class RequestTimeoutCustomException : Exception
    {
        public RequestTimeoutCustomException() : base() { }
        public RequestTimeoutCustomException(string message) : base(message) { }
        public RequestTimeoutCustomException(string statuscode, string message) : base(message) { }
        public RequestTimeoutCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
