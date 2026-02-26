using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RevenueModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public RevenueModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public decimal ServiceRevenue { get; set; }
        public int TotalBookings { get; set; }
        public List<Reservation> RecentTransactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. Lấy danh sách đặt phòng đã hoàn thành (CheckedOut)
            var completedReservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Customer)
                .Where(r => r.Status == "CheckedOut")
                .OrderByDescending(r => r.CheckOutDate)
                .ToListAsync();

            RecentTransactions = completedReservations.Take(10).ToList();
            TotalBookings = completedReservations.Count;

            // 2. Tính toán doanh thu phòng dựa trên số ngày ở * Giá phòng
            RoomRevenue = 0;
            foreach (var res in completedReservations)
            {
                var days = (res.CheckOutDate - res.CheckInDate).Days;
                if (days <= 0) days = 1; // Tính tối thiểu 1 ngày

                RoomRevenue += days * (res.Room?.BasePrice ?? 0);
            }

            // 3. Doanh thu dịch vụ
            var completedReservationIds = completedReservations.Select(r => r.Id).ToList();
            ServiceRevenue = await _context.ReservationServices
                .Where(s => completedReservationIds.Contains(s.ReservationId))
                .SumAsync(s => s.Quantity * s.UnitPrice);

            TotalRevenue = RoomRevenue + ServiceRevenue;
        }
    }
}