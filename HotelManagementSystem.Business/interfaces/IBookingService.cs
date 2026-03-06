using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using HotelManagementSystem.Business.service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IBookingService
    {
        Task<List<HotelService>> GetAvailableServicesAsync();
        Task<bool> ProcessBooking(BookingRequest request);
        Task<(int ReservationId, string OrderId)?> CreatePendingBookingAsync(BookingRequest request, decimal amount);
        Task<bool> ConfirmPaymentAsync(string orderId, string transactionId);
        Task<bool> FailPaymentAsync(string orderId);
        Task<List<Reservation>> GetCustomerReservationsAsync(int customerId);
        Task<(bool Success, string Message)> ProcessRefundAsync(int reservationId, int customerId, IMoMoService momoService);
        Task<List<(DateTime CheckIn, DateTime CheckOut)>> GetReservedPeriodsAsync(int roomId, int? excludeReservationId = null);
    }
}
