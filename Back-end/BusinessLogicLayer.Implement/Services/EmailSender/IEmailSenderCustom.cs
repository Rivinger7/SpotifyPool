using Data_Access_Layer.Entities;

namespace Business_Logic_Layer.Services.EmailSender
{
    public interface IEmailSenderCustom
    {
		Task SendEmailConfirmationAsync(User user, string subject, string message);
        Task SendEmailForgotPasswordAsync(User user, string subject, string message);
    }
}
