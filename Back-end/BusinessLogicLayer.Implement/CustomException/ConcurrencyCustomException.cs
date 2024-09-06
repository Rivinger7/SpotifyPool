using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi có xung đột trong việc truy cập dữ liệu đồng thời
    /// </summary>
    public class ConcurrencyCustomException : Exception
    {
        public ConcurrencyCustomException() : base() { }
        public ConcurrencyCustomException(string message) : base(message) { }
        public ConcurrencyCustomException(string statuscode, string message) : base(message) { }
        public ConcurrencyCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
