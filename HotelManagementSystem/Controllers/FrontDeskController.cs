using BLL.Interfaces; 
using BLL.Service;
using DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class FrontDeskController : Controller
    {
        private readonly FrontDeskService _frontDeskService;
        private readonly IReservationRepository _reservationRepo;
        // 1. Add Cleaning Service Interface
        private readonly IRoomCleaningService _cleaningService;

        // 2. Inject it in Constructor
        public FrontDeskController(
            FrontDeskService service,
            IReservationRepository repo,
            IRoomCleaningService cleaningService) // <--- Add parameter
        {
            _frontDeskService = service;
            _reservationRepo = repo;
            _cleaningService = cleaningService; // <--- Assign it
        }

        // Dashboard: Shows Arrivals & In-House Guests
        public IActionResult Index()
        {
            var model = new FrontDeskViewModel
            {
                Arrivals = _reservationRepo.GetTodayArrivals(),
                InHouse = _reservationRepo.GetActiveReservations()
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CheckIn(int id)
        {
            try
            {
                // 1. Safe Retrieval of User ID
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If ID is missing, force them to log in again
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToAction("Login", "Account");
                }

                int staffId = int.Parse(userIdString);

                _frontDeskService.CheckInGuest(id, staffId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(int id) // Changed to async Task
        {
            try
            {
                // 1. Safe Retrieval of User ID
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToAction("Login", "Account");
                }

                int staffId = int.Parse(userIdString);

                // Perform the standard Checkout logic
                _frontDeskService.CheckOutGuest(id, staffId);

                // --- NEW AUTOMATION LOGIC ---
                // 2. Get the reservation to find the RoomId
                // We fetch it again briefly or you can modify FrontDeskService to return the RoomId
                var reservation = await _reservationRepo.GetReservationWithDetailsAsync(id);

                if (reservation != null)
                {
                    // 3. Create the cleaning task & mark room dirty
                    await _cleaningService.CreatePendingCleaningAsync(reservation.RoomId);
                }
                // -----------------------------

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckoutPreview(int id)
        {
            var reservation = await _reservationRepo.GetReservationWithDetailsAsync(id);
            if (reservation == null) return NotFound();

            // 1. Find the active Check-In record (the one without a checkout time)
            var checkInRecord = reservation.CheckInOuts.FirstOrDefault(c => c.CheckOutTime == null);
            if (checkInRecord == null || checkInRecord.CheckInTime == null)
            {
                TempData["Error"] = "No active check-in record found for this guest.";
                return RedirectToAction("Index");
            }

            // 2. Calculate the Bill Preview
            var checkInTime = checkInRecord.CheckInTime.Value;
            var checkOutTime = DateTime.Now;

            // Logic: Round up to the next full day. Minimum 1 day.
            var duration = checkOutTime - checkInTime;
            int daysStayed = (int)Math.Ceiling(duration.TotalDays);
            if (daysStayed < 1) daysStayed = 1;

            decimal roomPrice = reservation.Room.Price;
            decimal totalPrice = roomPrice * daysStayed;

            // 3. Pass data to View using a simple ViewModel or ViewBag
            ViewBag.DaysStayed = daysStayed;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.CheckInTime = checkInTime;
            ViewBag.CheckOutTime = checkOutTime;

            return View(reservation);
        }

        [HttpGet]
        public IActionResult History(string? search, DateTime? start, DateTime? end)
        {
            // 1. Call the repo with the new filters
            var history = _reservationRepo.GetCheckedOutReservations(search, start, end);

            // 2. Save the search terms in ViewBag so the form remembers what you typed
            ViewBag.CurrentSearch = search;
            ViewBag.Start = start?.ToString("yyyy-MM-dd");
            ViewBag.End = end?.ToString("yyyy-MM-dd");

            return View(history);
        }

        // 2. Show a specific Invoice (Printable)
        public async Task<IActionResult> Invoice(int id)
        {
            var reservation = await _reservationRepo.GetReservationWithDetailsAsync(id);
            if (reservation == null || reservation.Status != DTOs.Enums.ReservationStatus.CheckedOut)
            {
                return NotFound();
            }

            // Find the completed Check-In/Out record to get the final price
            var record = reservation.CheckInOuts
                .OrderByDescending(c => c.CheckOutTime)
                .FirstOrDefault();

            ViewBag.CheckInTime = record?.CheckInTime;
            ViewBag.CheckOutTime = record?.CheckOutTime;
            ViewBag.TotalAmount = record?.TotalAmount ?? 0;
            ViewBag.StaffName = record?.CheckOutStaff?.FullName ?? "Unknown";

            return View(reservation);
        }
    }
}