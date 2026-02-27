using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize]
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
            RequestData = new BookingRequest
            {
                RoomId = roomId ?? 0,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1)
            };

            // If user is Consumer, pre-fill customer info and restrict editing
            if (User.IsInRole("Consumer"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer != null)
                {
                    RequestData.CustomerId = customer.Id;
                }
            }

            await LoadServicesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadServicesAsync();

            // Re-validate customer ID for consumer
            bool isConsumer = User.IsInRole("Consumer");
            if (isConsumer)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                    return Page();
                }
                // Force CustomerId to be the logged-in user's customer ID
                RequestData.CustomerId = customer.Id;
            }

            if (!ModelState.IsValid) return Page();

            var result = await _service.ProcessBooking(RequestData, isConsumer);

            if (result)
            {
                if (isConsumer)
                {
                    // Redirect to a confirmation page or Index with message
                    TempData["Message"] = "Đặt phòng thành công! Vui lòng chờ Admin xác nhận.";
                    return RedirectToPage("Index");
                }
                return RedirectToPage("Index");
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
