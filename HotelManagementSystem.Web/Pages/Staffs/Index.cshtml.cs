using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Admin")] // Đảm bảo chỉ Admin mới có quyền truy cập và thao tác
    public class IndexModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        public IndexModel(HotelManagementDbContext context)
        {
            _context = context;
        }

        // Danh sách nhân viên hiển thị trên giao diện
        public IList<HotelManagementSystem.Data.Models.Staff> StaffList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Lấy danh sách nhân viên và nạp thông tin User (FullName, Email, Username)
            // Sắp xếp theo ngày tạo mới nhất để Admin dễ quản lý
            StaffList = await _context.Staffs
                .Include(s => s.User)
                .OrderByDescending(s => s.User != null ? s.User.CreatedAt : DateTime.MinValue)
                .ToListAsync();
        }

        // Hàm xử lý Xóa nhân viên
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Tìm nhân viên kèm theo thông tin User tương ứng
            var staff = await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (staff != null)
            {
                // Xóa bản ghi trong bảng Staff
                _context.Staffs.Remove(staff);

                // Xóa luôn tài khoản User liên quan để tránh dữ liệu rác
                if (staff.User != null)
                {
                    _context.Users.Remove(staff.User);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Đã xóa nhân viên và tài khoản liên quan thành công.";
            }

            // Sau khi xóa, quay lại trang danh sách nhân viên
            return RedirectToPage("./Index");
        }
    }
}