using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface ICheckOutService
    {
        Task<bool> ExecuteCheckOut(int reservationId, int staffId);
    }
}
