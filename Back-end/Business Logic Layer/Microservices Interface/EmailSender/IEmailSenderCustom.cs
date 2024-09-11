using DataAccessLayer.Repository.Entities;
using Utility.Coding;

namespace BusinessLogicLayer.Interface.Microservices_Interface.EmailSender
{
    public interface IEmailSenderCustom
    {
        Task SendEmailConfirmationAsync(User user, string subject, string message);
        Task SendEmailForgotPasswordAsync(User user, Message message); //(User user, string subject, string message);
    }
}
