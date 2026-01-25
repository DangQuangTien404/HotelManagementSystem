using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetReservationsByUsernameAsync(string username);
        Task<Reservation?> GetReservationWithDetailsAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByRoomIdAsync(int roomId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<(DateTime Start, DateTime End)>> GetBookedDateRangesAsync(int roomId);
    }
}
