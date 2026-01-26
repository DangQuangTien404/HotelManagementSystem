using DAL.Interfaces;
using DTOs.Entities;
using DTOs.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByUsernameAsync(string username)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Include(r => r.ReservedByUser)
                .Where(r => r.Customer.Email == username || (r.ReservedByUser != null && r.ReservedByUser.Username == username))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
        public IEnumerable<Reservation> GetPendingReservations()
        {
            return _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                // FIX: Use Enum instead of string "Pending"
                .Where(r => r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToList();
        }
        public async Task<Reservation?> GetReservationWithDetailsAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Include(r => r.ReservedByUser)
                .Include(r => r.CheckInOuts)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetReservationsByRoomIdAsync(int roomId)
        {
            return await _context.Reservations
                .Where(r => r.RoomId == roomId)
                .ToListAsync();
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Check if there are any overlapping reservations
            var hasOverlap = await _context.Reservations
                .AnyAsync(r => r.RoomId == roomId &&
                    r.Status != ReservationStatus.Cancelled &&
                    ((checkIn >= r.CheckInDate && checkIn < r.CheckOutDate) ||
                     (checkOut > r.CheckInDate && checkOut <= r.CheckOutDate) ||
                     (checkIn <= r.CheckInDate && checkOut >= r.CheckOutDate)));

            return !hasOverlap;
        }

        public async Task<Reservation> CreateReservationIfAvailableAsync(Reservation reservation)
        {
            // Use a serializable transaction to ensure atomicity of check + insert
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    // Check availability with row-level locking by querying overlapping reservations
                    var hasOverlap = await _context.Reservations
                        .Where(r => r.RoomId == reservation.RoomId &&
                            r.Status != ReservationStatus.Cancelled &&
                            ((reservation.CheckInDate >= r.CheckInDate && reservation.CheckInDate < r.CheckOutDate) ||
                             (reservation.CheckOutDate > r.CheckInDate && reservation.CheckOutDate <= r.CheckOutDate) ||
                             (reservation.CheckInDate <= r.CheckInDate && reservation.CheckOutDate >= r.CheckOutDate)))
                        .AnyAsync();

                    if (hasOverlap)
                    {
                        throw new InvalidOperationException("Room is not available for the selected dates.");
                    }

                    // Insert the reservation
                    await _context.Reservations.AddAsync(reservation);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return reservation;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<IEnumerable<(DateTime Start, DateTime End)>> GetBookedDateRangesAsync(int roomId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.RoomId == roomId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.CheckOutDate >= DateTime.UtcNow.Date)
                .Select(r => new { r.CheckInDate, r.CheckOutDate })
                .ToListAsync();

            //  Filter out any potential nulls and explicitly access .Value
            return reservations
                .Where(r => r.CheckInDate.HasValue && r.CheckOutDate.HasValue)
                .Select(r => (r.CheckInDate!.Value, r.CheckOutDate!.Value));
        }
        //  Implement GetTodayArrivals
        public IEnumerable<Reservation> GetTodayArrivals()
        {
            return _context.Reservations
        .Include(r => r.Customer)
        .Include(r => r.Room)
        .Where(r => r.Status == ReservationStatus.Confirmed)
        .OrderBy(r => r.CheckInDate) // Sort by date so earliest appear first
        .ToList();
        }

        //  Implement GetActiveReservations (In-House Guests)
        public IEnumerable<Reservation> GetActiveReservations()
        {
            return _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                // FIX: Add this line to load the Check-In history (Staff & Time)
                .Include(r => r.CheckInOuts)
                .Where(r => r.Status == ReservationStatus.CheckedIn)
                .OrderBy(r => r.CheckOutDate)
                .ToList();
        }
        public IEnumerable<Reservation> GetCheckedOutReservations()
        {
            return _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Include(r => r.CheckInOuts) // Important for invoice details
                .Where(r => r.Status == ReservationStatus.CheckedOut)
                .OrderByDescending(r => r.CheckOutDate) // Show most recent first
                .ToList();
        }
    }
}
