using HotelManagementSystem.Business.service;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Customer")]
    public class PaymentModel : PageModel
    {
        private readonly BookingService _service;
        private readonly MoMoService _momoService;
        private readonly HotelManagementDbContext _context;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public BookingRequest RequestData { get; set; } = new();

        public Room? SelectedRoom { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<HotelService> SelectedServices { get; set; } = new();
        public decimal RoomTotal { get; set; }
        public decimal ServiceTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public int Nights { get; set; }
        public string VietQrUrl { get; set; } = string.Empty;

        public PaymentModel(
            BookingService service,
            MoMoService momoService,
            HotelManagementDbContext context,
            IConfiguration configuration)
        {
            _service = service;
            _momoService = momoService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var json = TempData["BookingRequest"] as string;
            if (string.IsNullOrEmpty(json)) return RedirectToPage("/Rooms");

            RequestData = JsonSerializer.Deserialize<BookingRequest>(json)!;
            TempData.Keep("BookingRequest");

            await LoadDataAsync();
            if (SelectedRoom == null) return RedirectToPage("/Rooms");

            CalculateTotal();
            BuildVietQrUrl();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer != null)
            {
                RequestData.CustomerId = customer.Id;
                CustomerName = customer.FullName;
            }

            var result = await _service.ProcessBooking(RequestData);
            if (result)
            {
                TempData.Remove("BookingRequest");
                TempData["SuccessMessage"] = "Đặt phòng thành công! Cảm ơn bạn đã thanh toán.";
                return RedirectToPage("/Rooms");
            }

            await LoadDataAsync();
            CalculateTotal();
            BuildVietQrUrl();
            ModelState.AddModelError("", "Đặt phòng không thành công! Vui lòng kiểm tra lại.");
            return Page();
        }

        public async Task<IActionResult> OnPostMoMoAsync()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer != null)
            {
                RequestData.CustomerId = customer.Id;
                CustomerName = customer.FullName;
            }

            await LoadDataAsync();
            if (SelectedRoom == null) return RedirectToPage("/Rooms");

            CalculateTotal();

            var pendingResult = await _service.CreatePendingBookingAsync(RequestData, DepositAmount);
            if (pendingResult == null)
            {
                BuildVietQrUrl();
                ModelState.AddModelError("", "Không thể tạo đặt phòng. Phòng có thể đã được đặt.");
                return Page();
            }

            var (reservationId, orderId) = pendingResult.Value;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = _configuration["MoMo:ReturnUrl"] ?? "/PaymentCallback";
            var ipnPath = _configuration["MoMo:IpnUrl"] ?? "/api/momo-ipn";

            var redirectUrl = $"{baseUrl}{returnUrl}";
            var ipnUrl = $"{baseUrl}{ipnPath}";

            var orderInfo = $"Thanh toan phong {SelectedRoom!.RoomNumber}";
            var amount = (long)DepositAmount;

            var momoResponse = await _momoService.CreatePaymentAsync(
                orderId, orderInfo, amount, redirectUrl, ipnUrl);

            if (momoResponse != null && momoResponse.ResultCode == 0
                && !string.IsNullOrEmpty(momoResponse.PayUrl))
            {
                TempData.Remove("BookingRequest");
                return Redirect(momoResponse.PayUrl);
            }

            await _service.FailPaymentAsync(orderId);
            BuildVietQrUrl();
            ModelState.AddModelError("",
                $"Không thể kết nối MoMo: {momoResponse?.Message ?? "Không có phản hồi"}");
            return Page();
        }

        private async Task LoadDataAsync()
        {
            SelectedRoom = RequestData.RoomId > 0
                ? await _context.Rooms.FindAsync(RequestData.RoomId)
                : null;

            var customer = await GetCurrentCustomerAsync();
            CustomerName = customer?.FullName ?? string.Empty;

            if (RequestData.SelectedServiceIds.Any())
            {
                SelectedServices = await _context.HotelServices
                    .Where(s => s.IsActive && RequestData.SelectedServiceIds.Contains(s.Id))
                    .ToListAsync();
            }
        }

        private void CalculateTotal()
        {
            Nights = Math.Max(1, (RequestData.CheckOutDate - RequestData.CheckInDate).Days);
            RoomTotal = (SelectedRoom?.BasePrice ?? 0) * Nights;
            ServiceTotal = SelectedServices.Sum(s => s.Price);
            TotalPrice = RoomTotal + ServiceTotal;
            DepositAmount = SelectedRoom?.BasePrice ?? 0; // 1-night deposit charged at booking
        }

        private void BuildVietQrUrl()
        {
            var bankId = _configuration["VietQR:BankId"] ?? "970422";
            var accountNo = _configuration["VietQR:AccountNo"] ?? "0000000000";
            var accountName = _configuration["VietQR:AccountName"] ?? "LUXURY HOTEL";
            var template = _configuration["VietQR:Template"] ?? "compact2";

            var description = $"Thanh toan phong {SelectedRoom?.RoomNumber}";
            var amount = (long)DepositAmount;

            VietQrUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png"
                      + $"?amount={amount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(accountName)}";
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
