using System.Net.Mail;
using System.Net;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using DataAccessLayer.Repository.Entities;
using BusinessLogicLayer.Interface.Microservices_Interface.EmailSender;
using Utility.Coding;
using SetupLayer.Setting.Microservices.EmailSender;

namespace BusinessLogicLayer.Implement.Microservices.EmailSender
{
    public class EmailSender(EmailSenderSetting emailSenderSetting, ILogger<EmailSender> logger) : IEmailSenderCustom
    {
        private readonly EmailSenderSetting _emailSenderSetting = emailSenderSetting;
        private readonly ILogger<EmailSender> _logger = logger;

        public async Task SendEmailConfirmationAsync(User user, string subject, string message)
        {
            // Get username from the email
            string email = user.Email ?? "NULL";
            string username = user.UserName;

            try
            {
                var smtpClient = new SmtpClient(_emailSenderSetting.Smtp.Host)
                {
                    Port = int.Parse(_emailSenderSetting.Smtp.Port),
                    Credentials = new NetworkCredential(_emailSenderSetting.Smtp.Username, _emailSenderSetting.Smtp.Password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSenderSetting.FromAddress, _emailSenderSetting.FromName),
                    Subject = subject,
                    Body = GetEmailBody(username, message),
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {email} successfully.");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"SMTP error while sending email to {email}: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while sending email to {email}: {ex.Message}");
            }
        }

        private string GetEmailBody(string username, string message)
        {
            return $@"
            <tbody>
                <tr><td colspan='1' style='height:24px'></td></tr>
                <tr><td align='center'>
                    <table cellpadding='0' cellspacing='0' width='100%'>
                        <tbody>
                            <tr>
                                <td width='72' height='100%'></td>
                                <td align='center'>
                                    <h1 style='font-size:32px;font-weight:500;'>Xác Minh Email Của Bạn</h1>
                                </td>
                                <td width='72' height='100%'></td>
                            </tr>
                        </tbody>
                    </table>
                </td></tr>
                <tr><td align='center'>
                    <table cellpadding='0' cellspacing='0' width='100%' bgcolor='#F9F9F9'>
                        <tbody>
                            <tr><td colspan='3' style='height:40px'></td></tr>
                            <tr>
                                <td width='38' height='100%'></td>
                                <td align='center'>
                                    <table width='100%' style='table-layout:fixed;'>
                                        <tbody>
                                            <tr><td><h2 style='font-size:25.63px;font-weight:700;'>{username}</h2></td></tr>
                                            <tr><td colspan='1' style='height:8px'></td></tr>
                                            <tr><td align='center'>
                                                <p style='font-weight:500;font-size:18px;line-height:140%;letter-spacing:-0.01em;color:#666;'>
                                                    Bạn đã xác nhận địa chỉ email của Tài khoản Dental Care. Vui lòng xác minh email để xác nhận.<br>
                                                    Nếu bạn không yêu cầu bất kỳ thay đổi nào, hãy xóa email này. Nếu có thắc mắc, vui lòng liên hệ 
                                                    <a href='mailto:Rivinger7@gmail.com' style='color:#bd2225;text-decoration:underline;' target='_blank'>Bộ Phận Hỗ Trợ Dental Care</a>.
                                                </p>
                                            </td></tr>
                                            <tr><td colspan='1' style='height:40px'></td></tr>
                                            <tr><td align='center'>
                                                <div>
                                                    <a href='{HtmlEncoder.Default.Encode(message)}' style='min-width:300px;background:#1376f8;border-radius:12.8px;padding:25.5px 19px 26.5px 19px;text-align:center;font-size:18px;font-weight:700;color:#fff;display:inline-block;text-decoration:none;line-height:120%' target='_blank'>
                                                        Xác Minh Email
                                                    </a>
                                                </div>
                                            </td></tr>
                                        </tbody>
                                    </table>
                                </td>
                                <td width='38' height='100%'></td>
                            </tr>
                            <tr><td colspan='3' style='height:48px'></td></tr>
                        </tbody>
                    </table>
                </td></tr>
                <tr><td align='center'>
                    <table cellpadding='0' cellspacing='0' width='100%' style='font-size:16px;text-align:center;line-height:140%;letter-spacing:-0.01em;color:#666;'>
                        <tbody>
                            <tr>
                                <td width='100' height='100%'></td>
                                <td align='center'>
                                    Nếu bạn không phải là người gửi yêu cầu này, hãy đổi mật khẩu tài khoản ngay lập tức để tránh việc bị truy cập trái phép.
                                </td>
                                <td width='100' height='100%'></td>
                            </tr>
                            <tr><td colspan='3' style='height:80px'></td></tr>
                        </tbody>
                    </table>
                </td></tr>
                <tr><td align='center'>
                    <table cellpadding='0' cellspacing='0' width='100%'>
                        <tbody>
                            <tr>
                                <td width='72' height='100%'></td>
                                <td align='center'>
                                    <table cellpadding='0' cellspacing='0' width='100%' style='font-size:11.24px;line-height:140%;letter-spacing:-0.01em;color:#999;'>
                                        <tbody>
                                            <tr>
                                                <td align='center'>
                                                    <table style='display:inline-table;width:auto;'>
                                                        <tbody>
                                                            <tr>
                                                                <td align='center'>
                                                                    <a href='https://localhost:7165/' style='color:#bd2225;text-decoration:underline;' target='_blank'>
                                                                        <img src='https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Dental%20Care%20Logo%2FDentalCare.png?alt=media&token=8854a154-1dde-4aa3-b573-f3c0aca83776' alt='Logo Dental Care' style='border:0;height:auto;width:100%' width='142'>
                                                                    </a>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr><td align='center'>
                                                <table width='100%'>
                                                    <tbody>
                                                        <tr><td colspan='1' style='height:48px'></td></tr>
                                                        <tr><td align='center'>
                                                            <table cellpadding='0' cellspacing='0' width='100%' style='table-layout:fixed;'>
                                                                <tbody>
                                                                    <tr>
                                                                        <td style='border:none;text-align:center;' width='44' height='44'>
                                                                            <a href='https://www.facebook.com/profile.php?id=100093052218614' style='color:#bd2225;text-decoration:underline;' target='_blank'>
                                                                                <img alt='Biểu tượng Facebook' src='https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Dental%20Care%20Logo%2Ffacebookmail.png?alt=media&token=b882dbb7-ec80-4461-b496-825dcd9dbaf3' style='border:0;line-height:100%;outline:none;width:44px;height:44px' width='44' height='44'>
                                                                            </a>
                                                                        </td>
                                                                        <td width='24' height='44'></td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </td></tr>
                                                    </tbody>
                                                </table>
                                            </td></tr>
                                        </tbody>
                                    </table>
                                </td>
                                <td width='72' height='100%'></td>
                            </tr>
                            <tr><td colspan='3' style='height:48px'></td></tr>
                        </tbody>
                    </table>
                </td></tr>
            </tbody>";
        }

        public async Task SendEmailForgotPasswordAsync(User user, Message message)//(User user, string subject, string message)
        {
            // Get username from the email
            string email = user.Email ?? "NULL";
            string username = user.UserName;

            try
            {
                var smtpClient = new SmtpClient(_emailSenderSetting.Smtp.Host)
                {
                    Port = int.Parse(_emailSenderSetting.Smtp.Port),
                    Credentials = new NetworkCredential(_emailSenderSetting.Smtp.Username, _emailSenderSetting.Smtp.Password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSenderSetting.FromAddress, _emailSenderSetting.FromName),
                    Subject = message.Subject,
                    Body = message.Content,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {email} successfully.");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"SMTP error while sending email to {email}: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while sending email to {email}: {ex.Message}");
            }
        }
    }
}
