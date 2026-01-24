using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize]
    public class RoomCleaningController : Controller
    {
        private readonly IRoomCleaningService _cleaningService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public RoomCleaningController(IRoomCleaningService cleaningService, IRoomService roomService, IUserService userService)
        {
            _cleaningService = cleaningService;
            _roomService = roomService;
            _userService = userService;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var cleanings = await _cleaningService.GetAllCleaningsAsync();
            return View(cleanings);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Assign()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            var staff = await _userService.GetStaffUsersAsync();

            ViewBag.Rooms = new SelectList(rooms, "Id", "RoomNumber");
            ViewBag.Staff = new SelectList(staff, "Id", "FullName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Assign(int roomId, int staffUserId)
        {
            await _cleaningService.AssignCleanerAsync(roomId, staffUserId);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Staff, Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var cleaning = await _cleaningService.GetCleaningByIdAsync(id);
            if (cleaning == null) return NotFound();

            ViewBag.Statuses = new SelectList(new[] { "Pending", "In Progress", "Completed" }, cleaning.Status);

            return View(cleaning);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Administrator")]
        public async Task<IActionResult> Edit(int id, string status)
        {
            // Hàm này giữ nguyên, chỉ update status
            await _cleaningService.UpdateStatusAsync(id, status);

            if (User.IsInRole("Administrator"))
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "StaffDashboard");
        }
    }
}
