using BusinessLogicLayer.ModelView.Microservice_Model_Views.EmailService;

namespace BusinessLogicLayer.Interface.Microservices_Interface.EmailService
{
    public interface IEmailService
    {
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="emailMetadata">EmailMetadata model</param>
        /// <param name="option">
        /// <remarks>
        /// 1 - Register Template<br/>
        /// 2 - OTP Sending Template <br/>
        /// 3 - Reset Password Template
        /// </remarks>
        /// </param>
        /// <returns></returns>
        Task SendAsync(EmailMetadata emailMetadata, int option);
    }
}
