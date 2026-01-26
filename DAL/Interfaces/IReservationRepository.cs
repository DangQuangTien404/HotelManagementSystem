using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        public IEnumerable<Reservation> GetPendingReservations();
        Task<IEnumerable<Reservation>> GetReservationsByUsernameAsync(string username);
        Task<Reservation?> GetReservationWithDetailsAsync(int id);
        Task<IEnumerable<Reservation>> GetReservationsByRoomIdAsync(int roomId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<(DateTime Start, DateTime End)>> GetBookedDateRangesAsync(int roomId);
        /// <summary>
        /// Atomically checks room availability and creates the reservation within a transaction.
        /// Throws InvalidOperationException if the room is not available for the specified dates.
        /// </summary>
        Task<Reservation> CreateReservationIfAvailableAsync(Reservation reservation);
        IEnumerable<Reservation> GetTodayArrivals();
        IEnumerable<Reservation> GetActiveReservations();
        ///invoice
        IEnumerable<Reservation> GetCheckedOutReservations();
    }
}
