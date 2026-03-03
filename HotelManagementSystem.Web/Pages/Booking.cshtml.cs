using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HotelManagementSystem.Web.Pages
{
    public class BookingModel : PageModel
    {
        private readonly BookingService _service;
        private readonly HotelManagementDbContext _context;

        [BindProperty]
        public BookingRequest RequestData { get; set; } = new();

        public List<HotelService> AvailableServices { get; set; } = new();
        public string CustomerName { get; set; } = string.Empty;
        public Room? SelectedRoom { get; set; }

        public BookingModel(BookingService service, HotelManagementDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            SelectedRoom = await LoadRoomAsync(roomId ?? 0);
            if (SelectedRoom == null) return RedirectToPage("/Rooms");

            var customer = await GetCurrentCustomerAsync();
            CustomerName = customer?.FullName ?? string.Empty;

            RequestData = new BookingRequest
            {
                CustomerId = customer?.Id ?? 0,
                RoomId = SelectedRoom.Id,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1)
            };

            await LoadServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Always resolve CustomerId from the logged-in user to prevent tampering
            var customer = await GetCurrentCustomerAsync();
            if (customer != null)
            {
                RequestData.CustomerId = customer.Id;
                CustomerName = customer.FullName;
            }

            SelectedRoom = await LoadRoomAsync(RequestData.RoomId);
            await LoadServicesAsync();

            if (!ModelState.IsValid) return Page();

            // Pass booking data to the payment page
            TempData["BookingRequest"] = JsonSerializer.Serialize(RequestData);
            return RedirectToPage("/Payment");
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task LoadServicesAsync()
        {
            AvailableServices = await _service.GetAvailableServicesAsync();
        }

        private async Task<Room?> LoadRoomAsync(int roomId)
        {
            if (roomId <= 0) return null;
            return await _context.Rooms.FindAsync(roomId);
        }
    }
}
