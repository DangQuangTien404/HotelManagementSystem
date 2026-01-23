using BLL.Interfaces;
using DTOs.Enums;
using HotelManagementSystem.ViewModels;
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
            
            var viewModel = new HomeIndexViewModel
            {
                Rooms = rooms,
                SearchTerm = searchTerm,
                SelectedRoomType = roomType,
                MaxPrice = maxPrice
            };
            
            return View(viewModel);
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
