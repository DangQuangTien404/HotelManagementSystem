using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public IndexModel(HotelManagementDbContext context) => _context = context;

        // Danh sách hiển thị ra giao diện
        public List<RoomViewModel> RoomDisplay { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Customer")
                return RedirectToPage("/Rooms");

            if (role == "Staff")
                return RedirectToPage("/Staffs/MyTasks");

            if (role == "Technician")
                return RedirectToPage("/Staffs/MaintenanceTasks");

            if (role == "Receptionist")
                return RedirectToPage("/Reservations");

            if (role != "Admin")
                return RedirectToPage("/Login");

            var rooms = await _context.Rooms.ToListAsync();

            // Lấy các đơn đặt phòng đang có khách (Status là CheckedIn)
            var activeReservations = await _context.Reservations
                .Where(r => r.Status == "CheckedIn")
                .ToListAsync();

            // Ánh xạ dữ liệu
            RoomDisplay = rooms.Select(r => new RoomViewModel
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Price = r.Price,
                Status = r.Status,
                // Tìm mã đơn đặt phòng tương ứng cho phòng này (nếu có khách)
                CurrentReservationId = activeReservations.FirstOrDefault(res => res.RoomId == r.Id)?.Id
            }).ToList();

            return Page();
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
