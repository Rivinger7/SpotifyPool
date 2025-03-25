using BusinessLogicLayer.Interface.Services_Interface.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Net.payOS;
using Net.payOS.Types;
using System.Text.Json;

namespace SpotifyPool._1._Controllers.Payment
{
    [Route("api/v1/payments")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // "Bearer"
    public class PaymentController(IPayment paymentService) : Controller
    {
        private readonly IPayment _paymentService = paymentService;

        [AllowAnonymous, HttpGet("premiums")]
        public async Task<IActionResult> GetPremiums()
        {
            try
            {
                var result = await _paymentService.GetAllPremiums();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous, HttpGet()]
        public async Task<IActionResult> GetPayments(string? userId)
        {
            try
            {
                var result = await _paymentService.GetAllPayments(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
		/// Tạo link thanh toán
		/// </summary>
		/// <param name="premiumId">Id gói bạn muốn đăng kí</param>
		/// <returns></returns>
        [Authorize(), HttpPost("payment-link")]
        public async Task<IActionResult> CreatePaymentLink(string premiumId)
        {
            try
            {
                var result = await _paymentService.CreatePaymentRequest(premiumId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous, HttpGet("payment-link")]
        public async Task<IActionResult> GetPaymentLink(long orderCode)
        {
            try
            {
                var result = await _paymentService.GetPaymantLinkInfo(orderCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous, HttpDelete("payment-link")]
        public async Task<IActionResult> DeletePaymentLink(long orderCode, string? reason)
        {
            try
            {
                var result = await _paymentService.CancelPaymantLink(orderCode, reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
		/// Api này tự chạy khi PayOS gửi webhook khi có sự kiện thanh toán xảy ra (thanh toán thành công, hủy,...)
		/// </summary>
        [AllowAnonymous, HttpPost("webhook-handle")]
        public async Task<IActionResult> HandleWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            string rawBody = await reader.ReadToEndAsync();

            try
            {
                // Deserialize JSON vào đúng kiểu WebhookType từ SDK
                WebhookType? webhookType = JsonSerializer.Deserialize<WebhookType>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // cho phép JSON camelCase
                });
                if (webhookType == null)
                {
                    return BadRequest("Dữ liệu webhook không hợp lệ hoặc null!");
                }
                await _paymentService.HandleWebhook(webhookType);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi xử lý webhook: " + ex.Message);
                return BadRequest("Invalid webhook");
            }
        }

        /// <summary>
		/// Cập nhật URL Webhook cho Kênh thanh toán
		/// </summary>
		/// <param name="url">vd: url = https://your-webhook-url/</param>
		/// <returns></returns>
        [AllowAnonymous, HttpPost("webhook-confirm")]
        public async Task<IActionResult> ConfirmWebhook(string url)
        {
            string result = await _paymentService.ConfirmWebhook(url); 
            return Ok(result);
        }
    }
}
