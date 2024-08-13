using Data_Access_Layer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Logic_Layer.Services.EmailSender
{
    public interface IEmailSenderCustom
    {
		Task SendEmailConfirmationAsync(User user, string subject, string message);
    }
}
