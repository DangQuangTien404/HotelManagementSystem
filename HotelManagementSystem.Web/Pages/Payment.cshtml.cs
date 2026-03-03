using HotelManagementSystem.Business;
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
        public int Nights { get; set; }
        public string VietQrUrl { get; set; } = string.Empty;

        public PaymentModel(BookingService service, HotelManagementDbContext context, IConfiguration configuration)
        {
            _service = service;
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
                TempData["SuccessMessage"] = "Đặt phòng thành công! Cảm ơn bạn đã thanh toán.";
                return RedirectToPage("/Rooms");
            }

            // Re-load for re-display on failure
            await LoadDataAsync();
            CalculateTotal();
            BuildVietQrUrl();
            ModelState.AddModelError("", "Đặt phòng không thành công! Vui lòng kiểm tra lại.");
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
        }

        private void BuildVietQrUrl()
        {
            var bankId = _configuration["VietQR:BankId"] ?? "970422";
            var accountNo = _configuration["VietQR:AccountNo"] ?? "0000000000";
            var accountName = _configuration["VietQR:AccountName"] ?? "LUXURY HOTEL";
            var template = _configuration["VietQR:Template"] ?? "compact2";

            var description = $"Thanh toan phong {SelectedRoom?.RoomNumber}";
            var amount = (long)TotalPrice;

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
