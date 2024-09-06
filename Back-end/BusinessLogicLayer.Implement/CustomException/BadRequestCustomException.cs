using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    public class BadRequestCustomException : Exception
    {
        public BadRequestCustomException() : base() { }
        public BadRequestCustomException(string message) : base(message) { }
        public BadRequestCustomException(string statuscode, string message) : base(message) { }
        public BadRequestCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
