using BLL.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IRoomService _roomService;

        public ReservationController(IReservationService reservationService, IRoomService roomService)
        {
            _reservationService = reservationService;
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomId)
        {
            var room = await _roomService.GetRoomByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            var model = new CreateReservationDto
            {
                RoomId = roomId
            };

            ViewBag.Room = room;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationDto model)
        {
            if (!ModelState.IsValid)
            {
                var room = await _roomService.GetRoomByIdAsync(model.RoomId);
                ViewBag.Room = room;
                return View(model);
            }

            try
            {
                var username = User.Identity?.Name ?? string.Empty;
                var reservation = await _reservationService.CreateReservationAsync(model, username);
                
                TempData["SuccessMessage"] = "Your reservation has been confirmed!";
                return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var room = await _roomService.GetRoomByIdAsync(model.RoomId);
                ViewBag.Room = room;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var reservations = await _reservationService.GetUserReservationsAsync(username);
            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _reservationService.CancelReservationAsync(id);
                TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(MyReservations));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnavailableDates(int roomId)
        {
            var unavailableDates = await _reservationService.GetUnavailableDatesAsync(roomId);
            var dateStrings = unavailableDates.Select(d => d.ToString("yyyy-MM-dd"));
            return Json(dateStrings);
        }
    }
}
