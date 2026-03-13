using HotelManagementSystem.Business.service;
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
        private readonly IMoMoService _momoService;
        private readonly IVnPayService _vnPayService;

        public bool IsSuccess { get; set; }
        public string ResultMessage { get; set; } = string.Empty;
        public string? OrderId { get; set; }

        public PaymentCallbackModel(
            IBookingService bookingService,
            IMoMoService momoService,
            IVnPayService vnPayService)
        {
            _bookingService = bookingService;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Request.Query.ContainsKey("vnp_TxnRef"))
            {
                return await HandleVnPayCallbackAsync();
            }

            return await HandleMoMoCallbackAsync();
        }

        private async Task<IActionResult> HandleVnPayCallbackAsync()
        {
            OrderId = Request.Query["vnp_TxnRef"].ToString();
            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var transactionNo = Request.Query["vnp_TransactionNo"].ToString();

            if (string.IsNullOrEmpty(OrderId))
            {
                IsSuccess = false;
                ResultMessage = "Thông tin thanh toán VNPay không hợp lệ.";
                return Page();
            }

            if (!_vnPayService.VerifySignature(Request.Query))
            {
                IsSuccess = false;
                ResultMessage = "Chữ ký VNPay không hợp lệ. Thanh toán không được xác nhận.";
                return Page();
            }

            if (responseCode == "00")
            {
                var confirmed = await _bookingService.ConfirmPaymentAsync(OrderId, transactionNo);
                IsSuccess = confirmed;
                ResultMessage = confirmed
                    ? "Thanh toán VNPay thành công! Đặt phòng của bạn đã được xác nhận."
                    : "Thanh toán thành công nhưng không thể cập nhật đặt phòng. Vui lòng liên hệ hỗ trợ.";
            }
            else
            {
                await _bookingService.FailPaymentAsync(OrderId);
                IsSuccess = false;
                ResultMessage = $"Thanh toán VNPay không thành công (mã lỗi: {responseCode}).";
            }

            return Page();
        }

        private async Task<IActionResult> HandleMoMoCallbackAsync()
        {
            var orderId = Request.Query["orderId"].ToString();
            var signature = Request.Query["signature"].ToString();
            var resultCode = ParseInt(Request.Query["resultCode"]);
            var amount = ParseLong(Request.Query["amount"]);
            var transId = ParseLong(Request.Query["transId"]);
            var responseTime = ParseLong(Request.Query["responseTime"]);

            OrderId = orderId;

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(signature))
            {
                IsSuccess = false;
                ResultMessage = "Thông tin thanh toán không hợp lệ.";
                return Page();
            }

            var callbackData = new MoMoCallbackData
            {
                PartnerCode = Request.Query["partnerCode"].ToString(),
                OrderId = orderId,
                RequestId = Request.Query["requestId"].ToString(),
                Amount = amount,
                OrderInfo = Request.Query["orderInfo"].ToString(),
                OrderType = Request.Query["orderType"].ToString(),
                TransId = transId,
                ResultCode = resultCode,
                Message = Request.Query["message"].ToString(),
                PayType = Request.Query["payType"].ToString(),
                ResponseTime = responseTime,
                ExtraData = Request.Query["extraData"].ToString(),
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
                ResultMessage = $"Thanh toán không thành công: {callbackData.Message}";
            }

            return Page();
        }

        private static int ParseInt(string value)
            => int.TryParse(value, out var number) ? number : 0;

        private static long ParseLong(string value)
            => long.TryParse(value, out var number) ? number : 0;
    }
}
