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
    public class AdminTaskController : Controller
    {
        private readonly IAdminTaskService _taskService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public AdminTaskController(
            IAdminTaskService taskService,
            IRoomService roomService,
            IUserService userService)
        {
            _taskService = taskService;
            _roomService = roomService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _taskService.GetAllMaintenanceTasksAsync();
            var rooms = await _roomService.GetAllRoomsAsync();
            var staff = await _userService.GetStaffUsersAsync();

            ViewBag.Rooms = new SelectList(rooms, "Id", "RoomNumber");
            ViewBag.Staff = new SelectList(staff, "Id", "FullName");
            ViewBag.Priorities = new SelectList(new[] { "High", "Medium", "Low" });

            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(MaintenanceTaskDto taskDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.AssignMaintenanceTaskAsync(taskDto);
                    TempData["SuccessMessage"] = "Task assigned successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // REMOVED: Inline editing replaced with dedicated Edit form
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTask(MaintenanceTaskDto taskDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.UpdateMaintenanceTaskAsync(taskDto);
                    TempData["SuccessMessage"] = "Task updated successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        */

        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetMaintenanceTaskByIdAsync(id);
            if (task == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction(nameof(Index));
            }

            if (task.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Cannot edit approved tasks.";
                return RedirectToAction(nameof(Index));
            }

            var rooms = await _roomService.GetAllRoomsAsync();
            var staff = await _userService.GetStaffUsersAsync();

            ViewBag.Rooms = new SelectList(rooms, "Id", "RoomNumber", task.RoomId);
            ViewBag.Staff = new SelectList(staff, "Id", "FullName", task.AssignedTo);
            ViewBag.Priorities = new SelectList(new[] { "High", "Medium", "Low" }, task.Priority);

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceTaskDto taskDto)
        {
            if (id != taskDto.Id)
            {
                TempData["ErrorMessage"] = "Invalid task ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _taskService.UpdateMaintenanceTaskAsync(taskDto);
                    TempData["SuccessMessage"] = "Task updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }

            var rooms = await _roomService.GetAllRoomsAsync();
            var staff = await _userService.GetStaffUsersAsync();
            ViewBag.Rooms = new SelectList(rooms, "Id", "RoomNumber", taskDto.RoomId);
            ViewBag.Staff = new SelectList(staff, "Id", "FullName", taskDto.AssignedTo);
            ViewBag.Priorities = new SelectList(new[] { "High", "Medium", "Low" }, taskDto.Priority);

            return View(taskDto);
        }

        // REMOVED: Admin should not manually update task status
        // Status should be updated by assigned staff only
        // Admin can only: Create, Edit task info, Approve when completed
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int taskId, string status)
        {
            try
            {
                await _taskService.UpdateTaskStatusAsync(taskId, status);
                TempData["SuccessMessage"] = "Task status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTask(int taskId)
        {
            try
            {
                var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _taskService.ApproveMaintenanceTaskAsync(taskId, adminId);
                TempData["SuccessMessage"] = "Task approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

