using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Pages.Reservations
{
    [Authorize(Roles = "Admin")]
    public class ServicesModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public ServicesModel(HotelManagementDbContext context) => _context = context;

        public Reservation Reservation { get; set; } = null!;
        public List<HotelService> AvailableServices { get; set; } = new();
        public List<ReservationService> SelectedServices { get; set; } = new();
        public decimal ServiceTotal => SelectedServices.Sum(s => s.Quantity * s.UnitPrice);

        [BindProperty]
        public int ServiceId { get; set; }

        [BindProperty]
        public int Quantity { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int reservationId)
        {
            var loaded = await LoadDataAsync(reservationId);
            if (!loaded) return RedirectToPage("/Reservations");
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(int reservationId)
        {
            if (Quantity <= 0) Quantity = 1;

            var reservation = await _context.Reservations.FindAsync(reservationId);
            var service = await _context.HotelServices.FirstOrDefaultAsync(s => s.Id == ServiceId && s.IsActive);
            if (reservation == null || service == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng hoặc dịch vụ.";
                return RedirectToPage(new { reservationId });
            }

            var existing = await _context.ReservationServices
                .FirstOrDefaultAsync(x => x.ReservationId == reservationId && x.HotelServiceId == ServiceId && x.UnitPrice == service.Price);

            if (existing != null)
            {
                existing.Quantity += Quantity;
            }
            else
            {
                _context.ReservationServices.Add(new ReservationService
                {
                    ReservationId = reservationId,
                    HotelServiceId = ServiceId,
                    Quantity = Quantity,
                    UnitPrice = service.Price,
                    AddedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Đã thêm dịch vụ vào hóa đơn.";
            return RedirectToPage(new { reservationId });
        }

        public async Task<IActionResult> OnPostRemoveAsync(int reservationId, int reservationServiceId)
        {
            var item = await _context.ReservationServices
                .FirstOrDefaultAsync(x => x.Id == reservationServiceId && x.ReservationId == reservationId);

            if (item != null)
            {
                _context.ReservationServices.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa dịch vụ khỏi hóa đơn.";
            }

            return RedirectToPage(new { reservationId });
        }

        private async Task<bool> LoadDataAsync(int reservationId)
        {
            Reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (Reservation == null) return false;

            AvailableServices = await _context.HotelServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            SelectedServices = await _context.ReservationServices
                .Include(s => s.HotelService)
                .Where(s => s.ReservationId == reservationId)
                .OrderByDescending(s => s.AddedAt)
                .ToListAsync();

            return true;
        }
    }
}
