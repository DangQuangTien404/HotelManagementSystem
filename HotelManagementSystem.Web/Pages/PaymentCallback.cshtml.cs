using HotelManagementSystem.Business.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class PaymentCallbackModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IStripeService _stripeService;

        public bool IsSuccess { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public string? OrderId { get; set; }

        public PaymentCallbackModel(
            IBookingService bookingService,
            IStripeService stripeService)
        {
            _bookingService = bookingService;
            _stripeService = stripeService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return await HandleStripeCallbackAsync();
        }

        private async Task<IActionResult> HandleStripeCallbackAsync()
        {
            OrderId = Request.Query["orderId"].ToString();
            var sessionId = Request.Query["session_id"].ToString();
            var cancelled = Request.Query["cancelled"].ToString();

            if (string.Equals(cancelled, "1", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(OrderId))
                {
                    await _bookingService.FailPaymentAsync(OrderId);
                }

                IsSuccess = false;
                ResultMessage = "Bạn đã hủy thanh toán Stripe.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(OrderId) || string.IsNullOrWhiteSpace(sessionId))
            {
                IsSuccess = false;
                ResultMessage = "Thông tin thanh toán Stripe không hợp lệ.";
                return Page();
            }

            var session = await _stripeService.GetSessionAsync(sessionId);
            if (session == null)
            {
                IsSuccess = false;
                ResultMessage = "Không thể xác minh phiên thanh toán Stripe. Vui lòng kiểm tra lại trong mục đặt phòng của bạn.";
                return Page();
            }

            if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                IsSuccess = false;
                ResultMessage = "Thanh toán đang được Stripe xử lý. Nếu đã trừ tiền, đặt phòng sẽ tự động cập nhật sau ít phút.";
                return Page();
            }

            var transactionId = session.PaymentIntent ?? session.Id;
            var confirmed = await _bookingService.ConfirmPaymentAsync(OrderId, transactionId);
            IsSuccess = confirmed;
            ResultMessage = confirmed
                ? "Thanh toán Stripe thành công! Đặt phòng của bạn đã được xác nhận."
                : "Thanh toán thành công nhưng không thể cập nhật đặt phòng. Vui lòng liên hệ hỗ trợ.";

            return Page();
        }
    }
}
