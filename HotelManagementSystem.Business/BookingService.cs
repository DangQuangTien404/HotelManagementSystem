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

        public async Task<List<HotelService>> GetAvailableServicesAsync()
        {
            return await _context.HotelServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> ProcessBooking(BookingRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null || room.Status != "Available") return false;

                var reservation = new Reservation
                {
                    RoomId = request.RoomId,
                    CustomerId = request.CustomerId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    Status = "Confirmed",
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
