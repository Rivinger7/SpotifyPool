using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi cố gắng tạo hoặc thêm dữ liệu đã tồn tại
    /// </summary>
    public class DataExistCustomException : Exception
    {
        public DataExistCustomException() : base() { }
        public DataExistCustomException(string message) : base(message) { }
        public DataExistCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
