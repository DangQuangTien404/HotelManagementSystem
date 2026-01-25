using DAL.Interfaces;
using DTOs.Entities;
using DTOs.Enums;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Reservation?> GetReservationWithDetailsAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Room)
                .Include(r => r.ReservedByUser)
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

        public async Task<IEnumerable<(DateTime Start, DateTime End)>> GetBookedDateRangesAsync(int roomId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.RoomId == roomId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.CheckOutDate >= DateTime.UtcNow.Date)
                .Select(r => new { r.CheckInDate, r.CheckOutDate })
                .ToListAsync();

            return reservations.Select(r => (r.CheckInDate, r.CheckOutDate));
        }
    }
}
