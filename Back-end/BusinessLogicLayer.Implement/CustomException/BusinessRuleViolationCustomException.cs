using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Implement.CustomException
{
    /// <summary>
    /// Xảy ra khi một quy tắc nghiệp vụ bị vi phạm
    /// </summary>
    public class BusinessRuleViolationCustomException : Exception
    {
        public BusinessRuleViolationCustomException() : base() { }
        public BusinessRuleViolationCustomException(string message) : base(message) { }
        public BusinessRuleViolationCustomException(string statuscode, string message) : base(message) { }
        public BusinessRuleViolationCustomException(string message, Exception innerException) : base(message, innerException) { }
    }
}
