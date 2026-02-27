using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Business;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        private readonly CheckOutService _checkOutService;

        public CheckoutModel(HotelManagementDbContext context, CheckOutService checkOutService)
        {
            _context = context;
            _checkOutService = checkOutService;
        }

        [BindProperty(SupportsGet = true)]
        public int ReservationId { get; set; }

        public Reservation Reservation { get; set; } = null!;
        public CheckInOut CheckInOutInfo { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        [BindProperty]
        public PaymentInputModel PaymentInput { get; set; } = new();

        public class PaymentInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập số thẻ")]
            [CreditCard(ErrorMessage = "Số thẻ không hợp lệ")]
            public string CardNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập ngày hết hạn (MM/YY)")]
            [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Ngày hết hạn phải theo định dạng MM/YY")]
            public string ExpiryDate { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập CVV")]
            [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV không hợp lệ")]
            public string CVV { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập tên chủ thẻ")]
            public string CardHolderName { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verify user owns this reservation
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return RedirectToPage("/Login");
            }

            Reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == ReservationId);

            if (Reservation == null) return NotFound();

            if (Reservation.Customer.UserId != currentUserId)
            {
                return Forbid();
            }

            if (Reservation.Status != "CheckedIn")
            {
                TempData["Error"] = "Đơn đặt phòng không ở trạng thái Check-in.";
                return RedirectToPage("/Index");
            }

            // Calculate estimated cost
            CheckInOutInfo = await _context.CheckInOuts
                .FirstOrDefaultAsync(c => c.ReservationId == ReservationId && c.CheckOutTime == null);

            if (CheckInOutInfo == null)
            {
                // Should not happen if status is CheckedIn
                TempData["Error"] = "Không tìm thấy thông tin Check-in.";
                return RedirectToPage("/Index");
            }

            var checkOutTime = DateTime.Now;
            var stayDuration = (checkOutTime - (CheckInOutInfo.CheckInTime ?? DateTime.Now)).Days;
            if (stayDuration <= 0) stayDuration = 1;

            var roomAmount = stayDuration * Reservation.Room.Price;
            var services = await _context.ReservationServices
                .Where(s => s.ReservationId == ReservationId)
                .ToListAsync();
            var serviceAmount = services.Sum(s => s.Quantity * s.UnitPrice);

            TotalAmount = roomAmount + serviceAmount;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload data for display
                await OnGetAsync();
                return Page();
            }

            // Verify user again
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return RedirectToPage("/Login");
            }

            // Call Service to process Checkout
            // Passing 0 or a flag to indicate system/user checkout if existing service requires staffId
            // Or better, update service to handle nullable staffId.
            // For now, I will assume I need to update the service first.
            // BUT, since I haven't updated the service yet in this plan step, I will mark a TODO or handle it in the service update step.
            // Actually, the plan says "Update Business Logic" is the next step.
            // So I will call a method `ExecuteUserCheckOut` that I WILL create in the next step.

            bool success = await _checkOutService.ExecuteUserCheckOut(ReservationId, currentUserId);

            if (success)
            {
                TempData["Message"] = "Thanh toán thành công! Cảm ơn quý khách.";
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình xử lý thanh toán.");
                await OnGetAsync();
                return Page();
            }
        }
    }
}
