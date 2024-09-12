using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Setting.Microservices.Jira
{
    public class JiraSetting
    {
        public string? UserName { get; set; } = string.Empty;
        public string? ApiKey { get; set; } = string.Empty;
        public string? Domain { get; set; } = string.Empty;
    }
}
