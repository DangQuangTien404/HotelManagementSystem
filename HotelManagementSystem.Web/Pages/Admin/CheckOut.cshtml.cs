using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CheckOutModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public CheckOutModel(HotelManagementDbContext context) => _context = context;

        [BindProperty]
        public Reservation Reservation { get; set; } = null!;
        public int TotalDays { get; set; }
        public decimal RoomTotalPrice { get; set; }
        public decimal ServiceTotalPrice { get; set; }
        public decimal TotalPrice => RoomTotalPrice + ServiceTotalPrice;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Reservation == null) return RedirectToPage("/Index");

            // Tính tiền
            var diff = DateTime.Now - Reservation.CheckInDate;
            TotalDays = diff.Days <= 0 ? 1 : diff.Days;
            RoomTotalPrice = TotalDays * (Reservation.Room?.BasePrice ?? 0);
            ServiceTotalPrice = await _context.ReservationServices
                .Where(s => s.ReservationId == Reservation.Id)
                .SumAsync(s => s.Quantity * s.UnitPrice);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var res = await _context.Reservations.Include(r => r.Room).FirstOrDefaultAsync(r => r.Id == id);
            if (res != null)
            {
                res.Status = "CheckedOut";
                res.CheckOutDate = DateTime.Now;
                if (res.Room != null) res.Room.Status = "Dirty"; // Trả phòng xong thì phòng bẩn

                await _context.SaveChangesAsync();
            }
            return RedirectToPage("/Index");
        }
    }
}
