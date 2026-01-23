using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Thêm thư viện này để dùng Where
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Staff")] // Hoặc Roles = "Admin" tùy bạn
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Cập nhật hàm Index để nhận từ khóa tìm kiếm và lọc Role
        public async Task<IActionResult> Index(string searchString, string roleFilter)
        {
            // 1. Lấy tất cả user
            var users = await _userService.GetAllUsersAsync();

            // 2. Lọc theo tên hoặc email nếu có searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                users = users.Where(u => u.FullName.ToLower().Contains(searchString)
                                      || u.Email.ToLower().Contains(searchString));
            }

            // 3. Lọc theo Role nếu có chọn
            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => u.Role == roleFilter);
            }

            // Lưu giữ giá trị để hiển thị lại trên View
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRole"] = roleFilter;

            return View(users);
        }

        // ... Các hàm Create, Edit, Delete giữ nguyên ...
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                await _userService.AddUserAsync(userDto);
                return RedirectToAction(nameof(Index));
            }
            return View(userDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserDto userDto)
        {
            if (id != userDto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(userDto);
                return RedirectToAction(nameof(Index));
            }
            return View(userDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}