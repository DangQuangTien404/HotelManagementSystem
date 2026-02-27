using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignMaintenanceModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public AssignMaintenanceModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaintenanceTask MaintenanceTask { get; set; } = new();

        public SelectList RoomList { get; set; }
        public SelectList TechStaffList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? roomId)
        {
            // Lấy danh sách các phòng chưa ở trạng thái bảo trì
            var rooms = await _context.Rooms
                .Where(r => r.Status != "Maintenance")
                .ToListAsync();

            RoomList = new SelectList(rooms, "Id", "RoomNumber", roomId);

            // Lấy danh sách nhân viên để giao việc (Role là Staff hoặc Admin)
            var staffUsers = await _context.Users
                .Where(u => u.Role == "Staff" || u.Role == "Admin")
                .ToListAsync();

            TechStaffList = new SelectList(staffUsers, "Id", "FullName");

            if (roomId.HasValue)
            {
                MaintenanceTask.RoomId = roomId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Nạp lại danh sách nếu có lỗi validation
                var rooms = await _context.Rooms.ToListAsync();
                RoomList = new SelectList(rooms, "Id", "RoomNumber");
                var techs = await _context.Users.ToListAsync();
                TechStaffList = new SelectList(techs, "Id", "FullName");
                return Page();
            }

            // Thiết lập thông tin mặc định cho Task
            MaintenanceTask.CreatedAt = DateTime.Now;
            MaintenanceTask.Status = "In Progress";

            _context.MaintenanceTasks.Add(MaintenanceTask);

            // Cập nhật trạng thái phòng sang Maintenance ngay lập tức
            var room = await _context.Rooms.FindAsync(MaintenanceTask.RoomId);
            if (room != null)
            {
                room.Status = "Maintenance";
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Đã phân công bảo trì cho phòng " + room?.RoomNumber;

            return RedirectToPage("/Index");
        }
    }
}
