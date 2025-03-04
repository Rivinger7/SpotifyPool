using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using BusinessLogicLayer.Interface.Services_Interface.BackgroundJobs.EmailSender;
using BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace BusinessLogicLayer.Implement.Services.BackgroundJobs.EmailSender
{
    public class BackgroundEmailSender(IEmailSenderCustom emailSenderCustom, Channel<IEmailSenderCustom> emailChannel) : BackgroundService, IBackgroundEmailSender
    {
        private readonly IEmailSenderCustom _emailSenderCustom = emailSenderCustom;
        private readonly Channel<IEmailSenderCustom> _emailChannel = emailChannel;

        public async Task QueueEmailAsync(EmailSenderRequestModel emailSenderRequestModel, Func<string> templateBody)
        {
            // Gán dữ liệu vào emailSenderCustom
            _emailSenderCustom.EmailSenderRequestModel = emailSenderRequestModel;
            _emailSenderCustom.TemplateBody = templateBody();

            // Gửi dữ liệu vào channel (Đẩy vào hàng chờ)
            await _emailChannel.Writer.WriteAsync(_emailSenderCustom);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Liên tục lắng nghe các yêu cầu từ Channel
            await foreach (IEmailSenderCustom emailRequest in _emailChannel.Reader.ReadAllAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                // Gửi email
                await emailRequest.SendEmailAsync(emailRequest.EmailSenderRequestModel);

                // Đợi 1-2 giây trước khi gửi email tiếp theo nếu có
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}