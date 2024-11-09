using BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request;

namespace BusinessLogicLayer.Interface.Services_Interface.BackgroundJobs.EmailSender
{
    public interface IBackgroundEmailSender
    {
        Task QueueEmailAsync(EmailSenderRequestModel emailSenderRequestModel, Func<string> templateBody);
    }
}
