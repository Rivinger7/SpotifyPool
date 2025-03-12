using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request;
using SetupLayer.Setting.Microservices.EmailSender;
using System.Net;
using System.Net.Mail;

namespace BusinessLogicLayer.Implement.Microservices.EmailSender
{
    public class EmailSender(EmailSenderSetting emailSenderSetting) : IEmailSenderCustom
    {
        private readonly EmailSenderSetting _emailSenderSetting = emailSenderSetting;
        
        public required EmailSenderRequestModel EmailSenderRequestModel { get; set; }
        public required string TemplateBody { get; set; }

        // Phương thức gửi email với thông tin từ EmailSenderRequestModel và templateBody
        private async Task SendEmailClient(EmailSenderRequestModel emailSenderRequestModel)
        {
            // Tạo đối tượng SmtpClient với thông tin từ EmailSenderSetting
            using SmtpClient smtpClient = new(_emailSenderSetting.Smtp.Host)
            {
                // Thiết lập cổng SMTP
                Port = int.Parse(_emailSenderSetting.Smtp.Port),

                // Thiết lập thông tin xác thực
                Credentials = new NetworkCredential(_emailSenderSetting.Smtp.Username, _emailSenderSetting.Smtp.Password),

                // Kích hoạt SSL
                EnableSsl = true,
            };

            // Tạo đối tượng MailMessage với thông tin từ EmailSenderRequestModel
            using MailMessage mailMessage = new()
            {
                // Thiết lập địa chỉ email người gửi
                From = new MailAddress(_emailSenderSetting.FromAddress, _emailSenderSetting.FromName),
                // Thiết lập tiêu đề email
                Subject = emailSenderRequestModel.Subject,
                // Thiết lập nội dung email
                Body = TemplateBody,
                // Thiết lập email có định dạng HTML
                IsBodyHtml = true,
            };

            // Có thể gửi email cho nhiều người nhận nếu có
            foreach (string recipientEmail in emailSenderRequestModel.EmailTo)
            {
                // Thêm địa chỉ email người nhận
                mailMessage.To.Add(recipientEmail);
            }

            // Gửi email
            await smtpClient.SendMailAsync(mailMessage);

            //// Ghi log thông tin email đã được gửi thành công
            //_logger.LogInformation($"Email đã được gửi tới {emailSenderRequestModel.EmailTo.Count()} người nhận thành công.");
            //_logger.LogInformation($"Email đã được gửi tới {emailSenderRequestModel.EmailTo}.");
        }

        public async Task SendEmailAsync(EmailSenderRequestModel emailSenderRequestModel)
        {
            await SendEmailClient(emailSenderRequestModel);
        }
    }
}
