using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignCleaningModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public AssignCleaningModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        // Khai báo các thuộc tính hiển thị
        public Room Room { get; set; } = default!;
        public List<HotelManagementSystem.Models.Staff> StaffList { get; set; } = new();

        // Thuộc tính nhận dữ liệu từ Form
        [BindProperty]
        public int SelectedStaffUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(int roomId)
        {
            // Lấy thông tin phòng
            Room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);

            if (Room == null)
            {
                return NotFound();
            }

            // Lấy danh sách nhân viên và thông tin User của họ để hiển thị tên
            StaffList = await _context.Staffs.Include(s => s.User).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return NotFound();

            // 1. Cập nhật trạng thái phòng sang "Cleaning" (Đang dọn)
            room.Status = "Cleaning";

            // 2. Tạo bản ghi mới trong bảng RoomCleaning
            var cleaningTask = new RoomCleaning
            {
                RoomId = roomId,
                CleanedBy = SelectedStaffUserId, // Lưu ID của User thực hiện
                CleaningDate = DateTime.Now,
                Status = "In Progress" // Trạng thái: Đang thực hiện
            };

            _context.RoomCleanings.Add(cleaningTask);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                // Load lại dữ liệu nếu lỗi
                Room = room;
                StaffList = await _context.Staffs.Include(s => s.User).ToListAsync();
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
