using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAdminCustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task AddCustomerAsync(CustomerDto customerDto);
        Task DeleteCustomerAsync(int id);
        Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId);
    }
}

