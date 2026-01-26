using DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetAllCustomersWithReservationsAsync();
        Task<Customer?> GetCustomerWithReservationsByIdAsync(int id);
    }
}

