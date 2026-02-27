using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Reservations
{
    [Authorize(Roles = "Admin")]
    public class ServicesModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public ServicesModel(HotelManagementService hotelService) => _hotelService = hotelService;

        public Reservation Reservation { get; set; } = null!;
        public List<HotelService> AvailableServices { get; set; } = new();
        public List<ReservationService> SelectedServices { get; set; } = new();
        public decimal ServiceTotal => SelectedServices.Sum(s => s.Quantity * s.UnitPrice);

        [BindProperty] public int ServiceId { get; set; }
        [BindProperty] public int Quantity { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int reservationId)
        {
            var loaded = await LoadDataAsync(reservationId);
            if (!loaded) return RedirectToPage("/Reservations");
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int reservationId)
        {
            if (Quantity <= 0) Quantity = 1;
            var ok = await _hotelService.AddServiceToReservationAsync(reservationId, ServiceId, Quantity);
            if (!ok) TempData["Error"] = "Không tìm thấy đặt phòng hoặc dịch vụ.";
            else TempData["Message"] = "Đã thêm dịch vụ vào hóa đơn.";
            return RedirectToPage(new { reservationId });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int reservationId, int reservationServiceId)
        {
            await _hotelService.RemoveReservationServiceAsync(reservationId, reservationServiceId);
            TempData["Message"] = "Đã xóa dịch vụ khỏi hóa đơn.";
            return RedirectToPage(new { reservationId });
        }

        private async Task<bool> LoadDataAsync(int reservationId)
        {
            Reservation = await _hotelService.GetReservationDetailAsync(reservationId);
            if (Reservation == null) return false;

            AvailableServices = await _hotelService.GetActiveServicesAsync();
            SelectedServices = await _hotelService.GetReservationServicesAsync(reservationId);
            return true;
        }
    }
}
