using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomDto roomDto)
        {
            if (ModelState.IsValid)
            {
                await _roomService.AddRoomAsync(roomDto);
                return RedirectToAction(nameof(Index));
            }
            return View(roomDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomDto roomDto)
        {
            if (id != roomDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _roomService.UpdateRoomAsync(roomDto);
                return RedirectToAction(nameof(Index));
            }
            return View(roomDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomService.DeleteRoomAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }
    }
}
