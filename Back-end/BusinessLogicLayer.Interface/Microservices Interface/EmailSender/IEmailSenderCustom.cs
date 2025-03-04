using BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request;

namespace BusinessLogicLayer.Interface.Microservices_Interface.EmailSender
{
    public interface IEmailSenderCustom
    {
        public EmailSenderRequestModel EmailSenderRequestModel { get; set; }
        public string TemplateBody { get; set; }

        Task SendEmailAsync(EmailSenderRequestModel emailSenderRequestModel);
    }
}
