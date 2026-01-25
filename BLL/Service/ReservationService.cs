using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using DTOs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<Customer> _customerRepository;
        private readonly IUserRepository _userRepository;

        public ReservationService(
            IReservationRepository reservationRepository,
            IGenericRepository<Room> roomRepository,
            IGenericRepository<Customer> customerRepository,
            IUserRepository userRepository)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
            _customerRepository = customerRepository;
            _userRepository = userRepository;
        }

        public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto reservationDto, string username)
        {
            if (reservationDto.CheckInDate < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Check-in date cannot be in the past.");
            }

            if (reservationDto.CheckOutDate <= reservationDto.CheckInDate)
            {
                throw new InvalidOperationException("Check-out date must be after check-in date.");
            }

            var isAvailable = await _reservationRepository.IsRoomAvailableAsync(
                reservationDto.RoomId, 
                reservationDto.CheckInDate, 
                reservationDto.CheckOutDate);

            if (!isAvailable)
            {
                throw new InvalidOperationException("Room is not available for the selected dates.");
            }

            var customer = await GetOrCreateCustomerAsync(reservationDto, username);

            var user = await _userRepository.GetByUsernameAsync(username);

            var room = await _roomRepository.GetByIdAsync(reservationDto.RoomId);
            if (room == null)
            {
                throw new InvalidOperationException("Room not found.");
            }

            var reservation = new Reservation
            {
                CustomerId = customer.Id,
                RoomId = reservationDto.RoomId,
                ReservedBy = user?.Id,
                CheckInDate = reservationDto.CheckInDate,
                CheckOutDate = reservationDto.CheckOutDate,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            await _reservationRepository.AddAsync(reservation);

            room.Status = RoomStatus.Reserved;
            await _roomRepository.UpdateAsync(room);

            var createdReservation = await _reservationRepository.GetReservationWithDetailsAsync(reservation.Id);
            return MapToDto(createdReservation!);
        }

        public async Task<IEnumerable<ReservationDto>> GetUserReservationsAsync(string username)
        {
            var reservations = await _reservationRepository.GetReservationsByUsernameAsync(username);
            return reservations.Select(MapToDto);
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
            return reservation != null ? MapToDto(reservation) : null;
        }

        public async Task CancelReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
            if (reservation == null)
            {
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                throw new InvalidOperationException("Reservation is already cancelled.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _reservationRepository.UpdateAsync(reservation);

            // Update room status back to Available
            var room = await _roomRepository.GetByIdAsync(reservation.RoomId);
            if (room != null && room.Status == RoomStatus.Reserved)
            {
                room.Status = RoomStatus.Available;
                await _roomRepository.UpdateAsync(room);
            }
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return await _reservationRepository.IsRoomAvailableAsync(roomId, checkIn, checkOut);
        }

        private async Task<Customer> GetOrCreateCustomerAsync(CreateReservationDto reservationDto, string username)
        {
            // Try to find existing customer by email
            var existingCustomers = await _customerRepository.GetAllAsync();
            var customer = existingCustomers.FirstOrDefault(c => c.Email == reservationDto.Email || c.Email == username);

            if (customer == null)
            {
                // Create new customer
                customer = new Customer
                {
                    FullName = reservationDto.FullName,
                    Email = string.IsNullOrWhiteSpace(reservationDto.Email) ? username : reservationDto.Email,
                    Phone = reservationDto.Phone,
                    IdentityNumber = reservationDto.IdentityNumber,
                    Address = reservationDto.Address,
                    CreatedAt = DateTime.UtcNow
                };
                await _customerRepository.AddAsync(customer);
            }

            return customer;
        }

        private static ReservationDto MapToDto(Reservation reservation)
        {
            var numberOfNights = (reservation.CheckOutDate - reservation.CheckInDate).Days;
            return new ReservationDto
            {
                Id = reservation.Id,
                CustomerId = reservation.CustomerId,
                CustomerName = reservation.Customer?.FullName ?? string.Empty,
                RoomId = reservation.RoomId,
                RoomNumber = reservation.Room?.RoomNumber ?? string.Empty,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Status = reservation.Status,
                NumberOfNights = numberOfNights,
                TotalPrice = (reservation.Room?.Price ?? 0) * numberOfNights
            };
        }
    }
}
