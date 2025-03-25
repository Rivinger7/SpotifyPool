
using DataAccessLayer.Repository.Entities;
using Net.payOS.Types;

namespace BusinessLogicLayer.Interface.Services_Interface.Payments
{
    public interface IPayment
    {
        Task<PaymentLinkInformation> CancelPaymantLink(long orderCode, string? reason);
        Task<string> ConfirmWebhook(string url);
        Task<CreatePaymentResult> CreatePaymentRequest(string premiumId);
        Task<IList<Payment>> GetAllPayments(string? userId);
        Task<IList<Premium>> GetAllPremiums();
        Task<PaymentLinkInformation> GetPaymantLinkInfo(long orderCode);
        Task HandleWebhook(WebhookType webhookType);
    }
}
