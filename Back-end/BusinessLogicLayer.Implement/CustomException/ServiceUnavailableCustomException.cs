using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi một dịch vụ quan trọng không khả dụng (thường là vấn đề từ phía server hoặc hạ tầng)
    /// </summary>
    public class ServiceUnavailableCustomException : Exception
    {
        public ServiceUnavailableCustomException() : base() { }
        public ServiceUnavailableCustomException(string message) : base(message) { }
        public ServiceUnavailableCustomException(string statuscode, string message) : base(message) { }
        public ServiceUnavailableCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
