using DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservationAsync(CreateReservationDto reservationDto, string username);
        Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(string username);
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task CancelReservationAsync(int id);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<DateTime>> GetUnavailableDatesAsync(int roomId);
    }
}
