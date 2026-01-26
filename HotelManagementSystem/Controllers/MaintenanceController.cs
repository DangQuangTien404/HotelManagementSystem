using BLL.Interfaces;
using DTOs.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Staff")]
    public class MaintenanceController : Controller
    {
        private readonly IRoomService _roomService;

        public MaintenanceController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // Hiển thị danh sách các phòng để quản lý bảo trì
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            // Chỉ lấy các phòng Đang bảo trì hoặc Có sẵn để thao tác cho gọn
            // Hoặc lấy tất cả để nhân viên dễ nhìn
            return View(rooms);
        }

        // Chuyển sang trạng thái Maintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartMaintenance(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room != null && room.Status == RoomStatus.Available)
            {
                await _roomService.UpdateRoomStatusAsync(id, RoomStatus.Maintenance);
            }
            return RedirectToAction(nameof(Index));
        }

        // Hoàn tất bảo trì, trả về Available
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishMaintenance(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room != null && room.Status == RoomStatus.Maintenance)
            {
                await _roomService.UpdateRoomStatusAsync(id, RoomStatus.Available);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}