using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi người dùng không có quyền thực hiện một thao tác cụ thể
    /// </summary>
    public class UnAuthorizedCustomException : Exception
    {
        public UnAuthorizedCustomException() : base() { }
        public UnAuthorizedCustomException(string message) : base(message) { }
        public UnAuthorizedCustomException(string statuscode, string message) : base(message) { }
        public UnAuthorizedCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
