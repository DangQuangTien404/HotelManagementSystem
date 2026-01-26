using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminNotificationController : Controller
    {
        private readonly IAdminNotificationService _notificationService;
        private readonly IUserService _userService;

        public AdminNotificationController(
            IAdminNotificationService notificationService,
            IUserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Only show incoming notifications from Customer and Staff (not from Admin)
            var notifications = await _notificationService.GetIncomingNotificationsAsync();
            var customers = await _userService.GetCustomerUsersAsync();
            var staff = await _userService.GetStaffUsersAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "FullName");
            ViewBag.Staff = new SelectList(staff, "Id", "FullName");

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToStaff(NotificationDto notificationDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                    await _notificationService.SendNotificationToStaffAsync(notificationDto, adminId);
                    TempData["SuccessMessage"] = "Notification sent to all staff successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToCustomer(NotificationDto notificationDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                    await _notificationService.SendNotificationToCustomerAsync(notificationDto, adminId);
                    TempData["SuccessMessage"] = notificationDto.RecipientId.HasValue 
                        ? "Notification sent to customer successfully." 
                        : "Notification sent to all customers successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(NotificationDto notificationDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                    await _notificationService.CreateAnnouncementAsync(notificationDto, adminId);
                    TempData["SuccessMessage"] = "Announcement created successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                TempData["SuccessMessage"] = "Notification marked as read.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

