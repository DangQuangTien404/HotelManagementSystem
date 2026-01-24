using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffDashboardController : Controller
    {
        private readonly IStaffDashboardService _dashboardService;

        public StaffDashboardController(IStaffDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return View(data);
        }
    }
}
