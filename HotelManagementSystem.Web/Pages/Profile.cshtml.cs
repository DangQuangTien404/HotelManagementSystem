using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly StaffService _staffService;
        private readonly HotelManagementDbContext _context; // Dùng để query nhanh task

        public ProfileModel(StaffService staffService, HotelManagementDbContext context)
        {
            _staffService = staffService;
            _context = context;
        }

        // Thay vì public Staff? StaffInfo { get; set; }
        public HotelManagementSystem.Data.Models.Staff? StaffInfo { get; set; }
        public List<MaintenanceTask> PendingMaintenanceTasks { get; set; } = new();
        public List<RoomCleaning> PendingCleaningTasks { get; set; } = new();
        public int TotalPendingTasks => PendingMaintenanceTasks.Count + PendingCleaningTasks.Count;

        public async Task OnGetAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return;
            }

            // 1. Lấy thông tin từ bảng Staff
            StaffInfo = await _staffService.GetStaffByUserId(userId);

            // 2. Lấy danh sách việc được giao (Maintenance)
            PendingMaintenanceTasks = await _context.MaintenanceTasks
                .Include(t => t.Room)
                .Where(t => t.AssignedTo == userId && t.Status != "Completed")
                .ToListAsync();

            PendingCleaningTasks = await _context.RoomCleanings
                .Include(c => c.Room)
                .Where(c => c.CleanedBy == userId && c.Status == "In Progress")
                .OrderBy(c => c.CleaningDate)
                .ToListAsync();
        }
    }
}