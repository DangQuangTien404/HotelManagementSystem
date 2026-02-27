using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OperationsModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public OperationsModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        // Thống kê số lượng phòng
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int OccupiedRooms { get; set; }

        // Danh sách tác vụ bảo trì và cảnh báo trễ hạn
        public List<MaintenanceTask> PendingTasks { get; set; } = new();

        // Giả sử bạn có bảng Reservation để kiểm tra Check-out trễ
        // Nếu chưa có bảng này, bạn có thể tạm comment phần OverdueCheckouts
        // public List<Reservation> OverdueCheckouts { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. Lấy dữ liệu thống kê phòng
            var allRooms = await _context.Rooms.ToListAsync();
            TotalRooms = allRooms.Count;
            AvailableRooms = allRooms.Count(r => r.Status == "Available");
            MaintenanceRooms = allRooms.Count(r => r.Status == "Maintenance");
            OccupiedRooms = allRooms.Count(r => r.Status == "Occupied");

            // 2. Lấy danh sách bảo trì chưa hoàn thành (Dùng đúng tên bảng MaintenanceTasks của bạn)
            PendingTasks = await _context.MaintenanceTasks
                .Include(m => m.Room)
                .Where(m => m.Status != "Completed")
                .OrderByDescending(m => m.Priority) // Ưu tiên hàng High lên đầu
                .ToListAsync();

            // 3. Logic cảnh báo trễ (Ví dụ)
            // OverdueCheckouts = await _context.Reservations
            //    .Include(r => r.Room).Include(r => r.Customer)
            //    .Where(r => r.CheckOutDate.Date == DateTime.Today && DateTime.Now.Hour >= 12)
            //    .ToListAsync();
        }

        // Hàm xử lý khi Admin bấm nút "Duyệt Hoàn Thành"
        public async Task<IActionResult> OnPostCompleteTaskAsync(int id)
        {
            var task = await _context.MaintenanceTasks
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task != null)
            {
                task.Status = "Completed";
                task.CompletedAt = DateTime.Now;

                // Nếu phòng đang bị khóa để bảo trì, mở khóa cho khách thuê
                if (task.Room != null && task.Room.Status == "Maintenance")
                {
                    task.Room.Status = "Available";
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Đã duyệt hoàn tất sửa chữa phòng {task.Room?.RoomNumber}";
            }

            return RedirectToPage();
        }
    }
}