using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi người dùng có xác thực nhưng không có quyền truy cập tài nguyên cụ thể
    /// </summary>
    public class ForbbidenCustomException : Exception
    {
        public ForbbidenCustomException() : base() { }
        public ForbbidenCustomException(string message) : base(message) { }
        public ForbbidenCustomException(string statuscode, string message) : base(message) { }
        public ForbbidenCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
