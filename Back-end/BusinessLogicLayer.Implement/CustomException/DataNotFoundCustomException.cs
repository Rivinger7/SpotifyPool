using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi dữ liệu cần thiết không được tìm thấy trong cơ sở dữ liệu hoặc nguồn dữ liệu khác
    /// </summary>
    public class DataNotFoundCustomException : Exception
    {
        public DataNotFoundCustomException() : base() { }
        public DataNotFoundCustomException(string message) : base(message) { }
        public DataNotFoundCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
