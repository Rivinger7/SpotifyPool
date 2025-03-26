using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Payments;
using DataAccessLayer.Implement.MongoDB.UOW;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Net.payOS;
using Net.payOS.Types;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Payments
{
    public class PaymentBLL(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor) : IPayment
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor = contextAccessor;
        private static string clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID")
            ?? throw new DataNotFoundCustomException("PAYOS_CLIENT_ID property is not set in environment or not found");
        private static string apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY")
            ?? throw new DataNotFoundCustomException("PAYOS_API_KEY property is not set in environment or not found");
        private static string checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY")
            ?? throw new DataNotFoundCustomException("PAYOS_CHECKSUM_KEY property is not set in environment or not found");
        private readonly PayOS _payOs = new PayOS(clientId, apiKey, checksumKey);
        public async Task<CreatePaymentResult> CreatePaymentRequest(string premiumId)
        {
            // UserID lấy từ phiên người dùng có thể là FE hoặc BE
            string? userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userID))
            {
                throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");
            }

            Premium premium = await _unitOfWork
                .GetCollection<Premium>()
                .Find(p => p.Id == premiumId).FirstOrDefaultAsync();
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds();
            ItemData item = new ItemData(premium.Name, 1, premium.Price);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);
            PaymentData paymentData = new PaymentData(
                orderCode,
                premium.Price,
                "Thanh toan don hang",
                items,
                cancelUrl: "https://spotifypoolmusicdevelopers.onrender.com/api/v1/payments/payment-link?orderCode=" + orderCode,
                returnUrl: "https://localhost:3002",
                expiredAt: expiredAt);
            // tạo link thanh toán
            CreatePaymentResult createPayment = await _payOs.createPaymentLink(paymentData);
            // tạo thông tin payment
            User user = await _unitOfWork.GetCollection<User>().Find(u => u.Id == userID).FirstOrDefaultAsync();
            Payment newPayment = new()
            {
                UserId = userID,
                PremiumId = premiumId,
                Status = "PENDING",
                OrderCode = orderCode,
                SnapshotInfo = new Snapshot()
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    CountryId = user.CountryId,
                    PhoneNumber = user.PhoneNumber,
                    Images = user.Images,
                    Birthdate = user.Birthdate,
                    Gender = user.Gender,
                    BuyedTime = Util.GetUtcPlus7Time(),
                    ExpiredTime = Util.GetUtcPlus7Time().AddDays(premium.Duration),
                    PremiumName = premium.Name,
                    PremiumPrice = premium.Price,
                    PremiumDuration = premium.Duration
                }
            };
            await _unitOfWork.GetCollection<Payment>().InsertOneAsync(newPayment);
            return createPayment;
        }
        public async Task<PaymentLinkInformation> GetPaymantLinkInfo(long orderCode)
        {
            return await _payOs.getPaymentLinkInformation(orderCode);
        }
        public async Task<PaymentLinkInformation> CancelPaymantLink(long orderCode, string? reason)
        {
            PaymentLinkInformation result = await _payOs.cancelPaymentLink(orderCode, reason);
            // Truy xuất dữ liệu
            Payment payment = await _unitOfWork.GetCollection<Payment>().Find(p => p.OrderCode == orderCode).FirstOrDefaultAsync();
            FilterDefinition<Payment> filter = Builders<Payment>.Filter.Eq(a => a.OrderCode, orderCode);
            UpdateDefinition<Payment> update = Builders<Payment>.Update.Set(a => a.Status, "CANCELLED");

            await _unitOfWork.GetCollection<Payment>().UpdateOneAsync(filter, update);

            Console.WriteLine($"Đã hủy đơn hàng: {orderCode}");
            return result;
        }
        public async Task<IList<Premium>> GetAllPremiums()
        {
            return await _unitOfWork.GetCollection<Premium>().Find(p => p.Id != null).ToListAsync();
        }
        public async Task<IList<Payment>> GetAllPayments(string? userId)
        {
            var filter = string.IsNullOrEmpty(userId)
        ? Builders<Payment>.Filter.Empty
        : Builders<Payment>.Filter.Eq(p => p.UserId, userId);

            return await _unitOfWork.GetCollection<Payment>().Find(filter).ToListAsync();
        }
        public async Task HandleWebhook(WebhookType webhookType)
        {
            // Xác minh dữ liệu
            WebhookData webhookData = _payOs.verifyPaymentWebhookData(webhookType);

            // Truy xuất dữ liệu
            Payment payment = await _unitOfWork.GetCollection<Payment>().Find(p => p.OrderCode == webhookData.orderCode).FirstOrDefaultAsync();
            if (payment == null)
            {
                Console.WriteLine($"Không tìm thấy đơn hàng với OrderCode: {webhookData.orderCode}");
                return;
            }
            FilterDefinition<Payment> filter = Builders<Payment>.Filter.Eq(a => a.OrderCode, webhookData.orderCode);
            UpdateDefinition<Payment> update;
            if (webhookData.code == "00") ///Thanh toán thành công
            {
                update = Builders<Payment>.Update.Set(a => a.Status, "PAID");
            }
            else // Hết thời gian thanh toán, lỗi hoặc bị từ chối
            {
                update = Builders<Payment>.Update.Set(a => a.Status, "CANCELLED");
                Console.WriteLine($"Đơn bị huỷ/lỗi: {webhookData.desc}");
            }

            await _unitOfWork.GetCollection<Payment>().UpdateOneAsync(filter, update);

            Console.WriteLine($"Đã thanh toán đơn hàng: {webhookData.orderCode}");
        }

        public async Task<string> ConfirmWebhook(string url)
        {
            return await _payOs.confirmWebhook(url);
        }
    }
}
