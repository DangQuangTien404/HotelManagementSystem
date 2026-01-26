using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AdminCustomerService : IAdminCustomerService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AdminCustomerService(
            IReservationRepository reservationRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _reservationRepository = reservationRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var users = await _userRepository.GetCustomerUsersAsync();
            var allReservations = await _reservationRepository.GetAllAsync();
            
            return users.Select(user =>
            {
                // Count reservations where ReservedBy = user.Id
                var userReservations = allReservations.Where(r => r.ReservedBy == user.Id).ToList();
                
                return new CustomerDto
                {
                    Id = user.Id,
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = string.Empty, // User doesn't have Phone field
                    IdentityNumber = string.Empty, // User doesn't have IdentityNumber field
                    Address = string.Empty, // User doesn't have Address field
                    CreatedAt = user.CreatedAt,
                    TotalReservations = userReservations.Count
                };
            });
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.Role != "Customer")
            {
                return null;
            }

            var allReservations = await _reservationRepository.GetAllAsync();
            var userReservations = allReservations.Where(r => r.ReservedBy == user.Id).ToList();

            return new CustomerDto
            {
                Id = user.Id,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = string.Empty,
                IdentityNumber = string.Empty,
                Address = string.Empty,
                CreatedAt = user.CreatedAt,
                TotalReservations = userReservations.Count
            };
        }

        public async Task AddCustomerAsync(CustomerDto customerDto)
        {
            // Check if username already exists
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Username == customerDto.Username))
            {
                throw new Exception("Username already exists.");
            }

            // Check if email already exists
            if (existingUsers.Any(u => u.Email == customerDto.Email))
            {
                throw new Exception("Email already exists.");
            }

            // Create User with Role = Customer
            var user = new User
            {
                Username = customerDto.Username,
                PasswordHash = _passwordHasher.HashPassword(customerDto.Password),
                Role = "Customer",
                FullName = customerDto.FullName,
                Email = customerDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            // Delete user account (customer is the user)
            await _userRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId)
        {
            // Get reservations where ReservedBy = customerId (which is UserId)
            var reservations = await _reservationRepository.GetReservationsByUserIdAsync(customerId);
            return reservations.Select(MapToReservationDto);
        }

        private ReservationDto MapToReservationDto(Reservation reservation)
        {
            return new ReservationDto
            {
                Id = reservation.Id,
                CustomerId = reservation.CustomerId,
                CustomerName = reservation.Customer?.FullName ?? string.Empty,
                RoomId = reservation.RoomId,
                RoomNumber = reservation.Room?.RoomNumber ?? string.Empty,
                RoomType = reservation.Room?.RoomType.ToString() ?? string.Empty,
                RoomPrice = reservation.Room?.Price ?? 0,
                ReservedBy = reservation.ReservedBy,
                ReservedByName = reservation.ReservedByUser?.FullName ?? "System",
                CheckInDate = reservation.CheckInDate ?? DateTime.MinValue,
                CheckOutDate = reservation.CheckOutDate ?? DateTime.MinValue,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }
    }
}

