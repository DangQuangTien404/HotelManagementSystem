using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business
{
    public class BookingService
    {
        private readonly HotelManagementDbContext _context;

        public BookingService(HotelManagementDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ProcessBooking(BookingRequest request, bool isConsumer = false)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var room = await _context.Rooms.FindAsync(request.RoomId);
                // If it's a consumer, we might still want to check availability,
                // but the room status change might be different or pending.
                // For simplicity, let's keep checking "Available".
                if (room == null || room.Status != "Available") return false;

                // Determine Status
                string status = isConsumer ? "Pending" : "Confirmed";

                var reservation = new Reservation
                {
                    RoomId = request.RoomId,
                    CustomerId = request.CustomerId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    Status = status,
                    CreatedAt = DateTime.Now
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                if (request.SelectedServiceIds.Any())
                {
                    var selectedServices = await _context.HotelServices
                        .Where(s => s.IsActive && request.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var service in selectedServices)
                    {
                        _context.ReservationServices.Add(new ReservationService
                        {
                            ReservationId = reservation.Id,
                            HotelServiceId = service.Id,
                            Quantity = 1,
                            UnitPrice = service.Price,
                            AddedAt = DateTime.Now
                        });
                    }
                }

                // If confirmed immediately (Staff/Admin), mark room as Reserved.
                // If Pending (Consumer), we might want to mark it as Reserved or keep it Available but maybe "Pending"?
                // Standard logic: If I book, no one else should take it. So let's mark it Reserved.
                // However, if admin rejects, we'll need to free it up.
                // Let's assume Pending bookings also reserve the room to prevent double booking.
                room.Status = "Reserved";
                _context.Rooms.Update(room);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
