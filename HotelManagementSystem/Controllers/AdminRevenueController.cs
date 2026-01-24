using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRevenueController : Controller
    {
        private readonly IRevenueService _revenueService;

        public AdminRevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        public async Task<IActionResult> Index(string periodType = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await _revenueService.GetRevenueReportAsync(startDate, endDate, periodType);
            
            ViewBag.PeriodType = periodType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? report.Revenue.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? report.Revenue.EndDate.ToString("yyyy-MM-dd");

            return View(report);
        }
    }
}

