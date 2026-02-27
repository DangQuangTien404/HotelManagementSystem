using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Staffs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public IndexModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        public IList<Staff> StaffList { get; set; } = default!;

        public async Task OnGetAsync()
        {
            StaffList = await _hotelService.GetStaffListAsync();
            StaffList = StaffList.OrderByDescending(s => s.User?.CreatedAt ?? DateTime.MinValue).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _hotelService.DeleteStaffAsync(id);
            TempData["Message"] = "Đã xóa nhân viên và tài khoản liên quan thành công.";
            return RedirectToPage("./Index");
        }
    }
}
