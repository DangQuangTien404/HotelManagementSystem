using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages
{
    // Allow Consumers to view the Index page to see rooms
    [Authorize(Roles = "Admin,Staff,Consumer")]
    public class IndexModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public IndexModel(HotelManagementDbContext context) => _context = context;

        // Danh sách hiển thị ra giao diện
        public List<RoomViewModel> RoomDisplay { get; set; } = new();

        public async Task OnGetAsync()
        {
            var rooms = await _context.Rooms.ToListAsync();

            // Lấy các đơn đặt phòng đang ở (Status là Booked hoặc CheckedIn)
            var activeReservations = await _context.Reservations
                .Where(r => r.Status == "Booked" || r.Status == "CheckedIn")
                .ToListAsync();

            // Ánh xạ dữ liệu
            RoomDisplay = rooms.Select(r => new RoomViewModel
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Price = r.BasePrice,
                Status = r.Status,
                // Tìm mã đơn đặt phòng tương ứng cho phòng này (nếu có khách)
                CurrentReservationId = activeReservations.FirstOrDefault(res => res.RoomId == r.Id)?.Id
            }).ToList();
        }

        // Lớp phụ trợ để tránh sửa Database
        public class RoomViewModel
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = null!;
            public string RoomType { get; set; } = null!;
            public decimal Price { get; set; }
            public string Status { get; set; } = null!;
            public int? CurrentReservationId { get; set; }
        }
    }
}
