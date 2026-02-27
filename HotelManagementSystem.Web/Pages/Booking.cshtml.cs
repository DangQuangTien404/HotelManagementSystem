using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize(Roles = "Consumer")]
    public class BookingModel : PageModel
    {
        private readonly BookingService _service;
        private readonly HotelManagementDbContext _context;

        [BindProperty]
        public BookingRequest RequestData { get; set; } = new();

        public List<HotelService> AvailableServices { get; set; } = new();

        public BookingModel(BookingService service, HotelManagementDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task OnGetAsync(int? roomId)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            RequestData = new BookingRequest
            {
                CustomerId = string.IsNullOrWhiteSpace(customerId) ? 0 : int.Parse(customerId),
                RoomId = roomId ?? 0,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1)
            };

            await LoadServicesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadServicesAsync();

            if (!ModelState.IsValid) return Page();

            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(customerId)) return RedirectToPage("/Login");
            RequestData.CustomerId = int.Parse(customerId);

            var result = await _service.ProcessBooking(RequestData);
            if (result)
            {
                TempData["Message"] = "Yêu cầu đặt phòng đã được gửi tới quản trị viên để xác nhận.";
                return RedirectToPage("/MyBookings");
            }

            ModelState.AddModelError("", "Đặt phòng không thành công! Vui lòng kiểm tra lại trạng thái phòng.");
            return Page();
        }

        private async Task LoadServicesAsync()
        {
            AvailableServices = await _context.HotelServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}
