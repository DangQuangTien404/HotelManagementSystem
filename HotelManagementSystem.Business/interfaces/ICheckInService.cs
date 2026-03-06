using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface ICheckInService
    {
        Task<bool> ExecuteCheckIn(int reservationId, int staffId);
    }
}
