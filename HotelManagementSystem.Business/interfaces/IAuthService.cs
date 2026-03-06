using HotelManagementSystem.Data.Models;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IAuthService
    {
        Task<User?> Login(string username, string password);
    }
}
