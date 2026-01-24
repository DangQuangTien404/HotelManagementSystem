using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminStaffController : Controller
    {
        private readonly IAdminStaffService _staffService;

        public AdminStaffController(IAdminStaffService staffService)
        {
            _staffService = staffService;
        }

        public async Task<IActionResult> Index()
        {
            var staffs = await _staffService.GetAllStaffAsync();
            return View(staffs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffDto staffDto)
        {
            // Validate password on create
            if (string.IsNullOrEmpty(staffDto.Password) || staffDto.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password is required and must be at least 6 characters.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _staffService.AddStaffAsync(staffDto);
                    TempData["SuccessMessage"] = "Staff created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(staffDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffDto staffDto)
        {
            if (id != staffDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _staffService.UpdateStaffAsync(staffDto);
                    TempData["SuccessMessage"] = "Staff updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(staffDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _staffService.DeleteStaffAsync(id);
                TempData["SuccessMessage"] = "Staff deleted successfully.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

