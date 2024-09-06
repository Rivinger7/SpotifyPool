using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi dữ liệu nhập vào hoặc dữ liệu lấy được từ nguồn không hợp lệ
    /// </summary>
    public class InvalidDataCustomException : Exception
    {
        public InvalidDataCustomException() : base() { }
        public InvalidDataCustomException(string message) : base(message) { }
        public InvalidDataCustomException(string statuscode, string message) : base(message) { }
        public InvalidDataCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
