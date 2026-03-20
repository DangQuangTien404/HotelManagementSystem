using HotelManagementSystem.Business.interfaces;
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
        private readonly IBookingService _service;
        private readonly IStripeService _stripeService;
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

        public PaymentModel(
            IBookingService service,
            IStripeService stripeService,
            HotelManagementDbContext context,
            IConfiguration configuration)
        {
            _service = service;
            _stripeService = stripeService;
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
            return Page();
        }

        public async Task<IActionResult> OnPostStripeAsync()
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

            if (string.IsNullOrWhiteSpace(_configuration["Stripe:SecretKey"]) || _configuration["Stripe:SecretKey"] == "YOUR_STRIPE_SECRET_KEY")
            {
                ModelState.AddModelError("", "Stripe chưa được cấu hình. Vui lòng liên hệ quản trị viên.");
                return Page();
            }

            var pendingResult = await _service.CreatePendingBookingAsync(
                RequestData,
                DepositAmount,
                paymentMethod: "Stripe",
                orderPrefix: "STRIPE");
            if (pendingResult == null)
            {
                ModelState.AddModelError("", "Không thể tạo đặt phòng. Phòng có thể đã được đặt.");
                return Page();
            }

            var (_, orderId) = pendingResult.Value;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var callbackPath = _configuration["Stripe:ReturnUrl"] ?? "/PaymentCallback";
            var successUrl = $"{baseUrl}{callbackPath}?provider=stripe&orderId={Uri.EscapeDataString(orderId)}&session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{baseUrl}{callbackPath}?provider=stripe&orderId={Uri.EscapeDataString(orderId)}&cancelled=1";

            var session = await _stripeService.CreateCheckoutSessionAsync(
                orderId,
                $"Thanh toan phong {SelectedRoom.RoomNumber}",
                (long)DepositAmount,
                successUrl,
                cancelUrl);

            if (session != null && !string.IsNullOrWhiteSpace(session.Url))
            {
                TempData.Remove("BookingRequest");
                return Redirect(session.Url);
            }

            await _service.FailPaymentAsync(orderId);
            ModelState.AddModelError("", "Không thể tạo phiên thanh toán Stripe.");
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
            RoomTotal = (SelectedRoom?.Price ?? 0) * Nights;
            ServiceTotal = SelectedServices.Sum(s => s.Price);
            TotalPrice = RoomTotal + ServiceTotal;
            DepositAmount = SelectedRoom?.Price ?? 0;
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
