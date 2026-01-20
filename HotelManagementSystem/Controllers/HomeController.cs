using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
