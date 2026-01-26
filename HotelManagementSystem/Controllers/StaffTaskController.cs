using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffTaskController : Controller
    {
        private readonly IStaffTaskService _staffTaskService;

        public StaffTaskController(IStaffTaskService staffTaskService)
        {
            _staffTaskService = staffTaskService;
        }


        public async Task<IActionResult> MyTasks()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var tasks = await _staffTaskService.GetTasksByStaffIdAsync(currentUserId);
                return View(tasks);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading tasks: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int taskId, string status)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _staffTaskService.UpdateMyTaskStatusAsync(taskId, currentUserId, status);
                TempData["SuccessMessage"] = "Task status updated successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not authorized to update this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating task: {ex.Message}";
            }

            return RedirectToAction(nameof(MyTasks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _staffTaskService.CompleteMyTaskAsync(taskId, currentUserId);
                TempData["SuccessMessage"] = "Task marked as completed. Waiting for admin approval.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not authorized to complete this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing task: {ex.Message}";
            }

            return RedirectToAction(nameof(MyTasks));
        }

 
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new Exception("User ID not found in claims.");
            }

            return int.Parse(userIdClaim);
        }
    }
}
