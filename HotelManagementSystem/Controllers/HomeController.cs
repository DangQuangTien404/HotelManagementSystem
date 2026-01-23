using BLL.Interfaces;
using DTOs.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;

        public HomeController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index(string? searchTerm, RoomType? roomType, decimal? maxPrice)
        {
            var rooms = await _roomService.SearchAvailableRoomsAsync(searchTerm, roomType, maxPrice);
            
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoomType = roomType;
            ViewBag.MaxPrice = maxPrice;
            
            return View(rooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
