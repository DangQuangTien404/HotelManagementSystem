using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    public class BookingModel : PageModel
    {
        private readonly BookingService _service;

        [BindProperty]
        public BookingRequest RequestData { get; set; } = new();

        public List<HotelService> AvailableServices { get; set; } = new();

        public BookingModel(BookingService service)
        {
            _service = service;
        }

        public async Task OnGetAsync(int? roomId)
        {
            RequestData = new BookingRequest
            {
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

            var result = await _service.ProcessBooking(RequestData);
            if (result) return RedirectToPage("Index");

            ModelState.AddModelError("", "Đặt phòng không thành công! Vui lòng kiểm tra lại trạng thái phòng.");
            return Page();
        }

        private async Task LoadServicesAsync()
        {
            AvailableServices = await _service.GetAvailableServicesAsync();
        }
    }
}
