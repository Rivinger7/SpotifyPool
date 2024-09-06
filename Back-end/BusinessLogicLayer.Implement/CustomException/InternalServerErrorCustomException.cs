using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi có lỗi chưa xác định trong hệ thống
    /// </summary>
    public class InternalServerErrorCustomException : Exception
    {
        public InternalServerErrorCustomException() : base() { }
        public InternalServerErrorCustomException(string message) : base(message) { }
        public InternalServerErrorCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
