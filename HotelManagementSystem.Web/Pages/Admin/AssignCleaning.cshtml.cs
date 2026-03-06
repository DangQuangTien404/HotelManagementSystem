using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Business.interfaces;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AssignCleaningModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IRoomUpdateBroadcaster _roomUpdateBroadcaster;

        public AssignCleaningModel(HotelManagementDbContext context, INotificationService notificationService, IRoomUpdateBroadcaster roomUpdateBroadcaster)
        {
            _context = context;
            _notificationService = notificationService;
            _roomUpdateBroadcaster = roomUpdateBroadcaster;
        }

        // Khai báo các thuộc tính hiển thị
        public Room Room { get; set; } = default!;
        public List<HotelManagementSystem.Data.Models.Staff> StaffList { get; set; } = new();

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

                // Send notification to the assigned staff
                var notification = new Notification
                {
                    RecipientId = SelectedStaffUserId,
                    SenderName = "Hệ thống quản lý",
                    SenderType = "System",
                    RecipientType = "Staff",
                    Message = $"Bạn có nhiệm vụ dọn dẹp mới tại phòng {room.RoomNumber}",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                await _notificationService.CreateAndSendNotificationAsync(notification, toAdminGroup: false);

                // Broadcast room status change
                await _roomUpdateBroadcaster.BroadcastRoomStatusAsync(room.Id, room.RoomNumber, room.Status);
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
