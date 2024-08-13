using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services.EmailSender
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailSenderSetting
    {
        public SmtpSettings Smtp { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }
}
