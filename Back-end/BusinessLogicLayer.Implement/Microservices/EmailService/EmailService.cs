using BusinessLogicLayer.Interface.Microservices_Interface.EmailService;
using BusinessLogicLayer.ModelView.Microservice_Model_Views.EmailService;
using FluentEmail.Core;
using Path = System.IO.Path;

namespace BusinessLogicLayer.Implement.Microservices.EmailService
{
    public class EmailService(IFluentEmail fluentEmail) : IEmailService
    {
        IFluentEmail _fluentEmail = fluentEmail ?? throw new ArgumentNullException(nameof(fluentEmail));

        public async Task SendAsync(EmailMetadata emailMetadata, int option)
        {
            string basePath = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? Directory.GetCurrentDirectory();

            string templatePath =  option switch
            {
                1 => $"{Path.Combine(basePath, "BusinessLogicLayer.Implement", "Microservices", "EmailService", "Template", "RegisterConfirmationTemplate.cshtml")}",

                2 => $"{Path.Combine(basePath, "BusinessLogicLayer.Implement", "Microservices", "EmailService", "Template", "OTPTemplate.cshtml")}",

                3 => $"{Path.Combine(basePath, "BusinessLogicLayer.Implement", "Microservices", "EmailService", "Template", "ResetPasswordTemplate.cshtml")}",

                _ => throw new ArgumentException("Invalid option", nameof(option))
            };

        await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .UsingTemplateFromFile(templatePath, emailMetadata)
            .SendAsync();
        }
    }
}
