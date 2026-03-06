using HotelManagementSystem.Data.Models;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IAccountService
    {
        Task<bool> RegisterStaff(User newUser, string position, string shift);
        Task<bool> RegisterCustomer(User newUser, Customer customer);
    }
}
