using HotelManagementSystem.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class PaymentCallbackModel : PageModel
    {
        private readonly BookingService _bookingService;
        private readonly MoMoService _momoService;

        public bool IsSuccess { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public string? OrderId { get; set; }

        public PaymentCallbackModel(BookingService bookingService, MoMoService momoService)
        {
            _bookingService = bookingService;
            _momoService = momoService;
        }

        public async Task<IActionResult> OnGetAsync(
            string? partnerCode, string? orderId, string? requestId,
            long amount, string? orderInfo, string? orderType,
            long transId, int resultCode, string? message,
            string? payType, long responseTime, string? extraData,
            string? signature)
        {
            OrderId = orderId;

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(signature))
            {
                IsSuccess = false;
                ResultMessage = "Thông tin thanh toán không hợp lệ.";
                return Page();
            }

            var callbackData = new MoMoCallbackData
            {
                PartnerCode = partnerCode ?? string.Empty,
                OrderId = orderId,
                RequestId = requestId ?? string.Empty,
                Amount = amount,
                OrderInfo = orderInfo ?? string.Empty,
                OrderType = orderType ?? string.Empty,
                TransId = transId,
                ResultCode = resultCode,
                Message = message ?? string.Empty,
                PayType = payType ?? string.Empty,
                ResponseTime = responseTime,
                ExtraData = extraData ?? string.Empty,
                Signature = signature
            };

            if (!_momoService.VerifySignature(callbackData))
            {
                IsSuccess = false;
                ResultMessage = "Chữ ký không hợp lệ. Thanh toán không được xác nhận.";
                return Page();
            }

            if (resultCode == 0)
            {
                var confirmed = await _bookingService.ConfirmPaymentAsync(orderId, transId.ToString());
                IsSuccess = confirmed;
                ResultMessage = confirmed
                    ? "Thanh toán MoMo thành công! Đặt phòng của bạn đã được xác nhận."
                    : "Thanh toán thành công nhưng không thể cập nhật đặt phòng. Vui lòng liên hệ hỗ trợ.";
            }
            else
            {
                await _bookingService.FailPaymentAsync(orderId);
                IsSuccess = false;
                ResultMessage = $"Thanh toán không thành công: {message}";
            }

            return Page();
        }
    }
}
