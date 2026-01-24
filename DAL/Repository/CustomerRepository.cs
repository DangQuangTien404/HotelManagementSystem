using DAL;
using DAL.Interfaces;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(HotelDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersWithReservationsAsync()
        {
            return await _context.Customers
                .Include(c => c.Reservations)
                    .ThenInclude(r => r.Room)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerWithReservationsByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Reservations)
                    .ThenInclude(r => r.Room)
                .Include(c => c.Reservations)
                    .ThenInclude(r => r.ReservedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

