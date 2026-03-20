using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Business.interfaces;

namespace HotelManagementSystem.Business.service
{
    public class BookingService : IBookingService
    {
        private readonly HotelManagementDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IRoomUpdateBroadcaster _roomUpdateBroadcaster;
        private readonly IReservationUpdateBroadcaster _reservationBroadcaster;

        public BookingService(HotelManagementDbContext context, INotificationService notificationService, IRoomUpdateBroadcaster roomUpdateBroadcaster, IReservationUpdateBroadcaster reservationBroadcaster)
        {
            _context = context;
            _notificationService = notificationService;
            _roomUpdateBroadcaster = roomUpdateBroadcaster;
            _reservationBroadcaster = reservationBroadcaster;
        }

        public async Task<List<HotelService>> GetAvailableServicesAsync()
        {
            return await _context.HotelServices
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<bool> ProcessBooking(BookingRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null) return false;

                if (await HasDateConflictAsync(request.RoomId, request.CheckInDate, request.CheckOutDate))
                    return false;

                var reservation = new Reservation
                {
                    RoomId = request.RoomId,
                    CustomerId = request.CustomerId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    Status = "CheckedIn",
                    CreatedAt = DateTime.Now
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                room.Status = "Occupied";

                _context.CheckInOuts.Add(new CheckInOut
                {
                    ReservationId = reservation.Id,
                    CheckInTime = DateTime.Now,
                    TotalAmount = 0
                });

                await _context.SaveChangesAsync();

                if (request.SelectedServiceIds.Any())
                {
                    var selectedServices = await _context.HotelServices
                        .Where(s => s.IsActive && request.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var service in selectedServices)
                    {
                        _context.ReservationServices.Add(new ReservationService
                        {
                            ReservationId = reservation.Id,
                            HotelServiceId = service.Id,
                            Quantity = 1,
                            UnitPrice = service.Price,
                            AddedAt = DateTime.Now
                        });
                    }
                }

                _context.Payments.Add(new Payment
                {
                    ReservationId = reservation.Id,
                    PaymentMethod = "VietQR",
                    OrderId = $"VIETQR_{reservation.Id}_{DateTime.Now:yyyyMMddHHmmss}",
                    Amount = CalculateTotal(room, request, await GetServicePricesAsync(request.SelectedServiceIds)),
                    Status = "Completed",
                    CreatedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();

                // Notify Admin
                await _notificationService.CreateAndSendNotificationAsync(new Notification
                {
                    Message = $"Đơn đặt phòng mới (VietQR): Phòng {room.RoomNumber}",
                    SenderName = "Hệ thống",
                    SenderType = "System",
                    RecipientType = "Admin",
                    IsAnnouncement = true
                }, toAdminGroup: true);

                await _roomUpdateBroadcaster.BroadcastRoomStatusAsync(room.Id, room.RoomNumber, room.Status);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<(int ReservationId, string OrderId)?> CreatePendingBookingAsync(
            BookingRequest request,
            decimal amount,
            string paymentMethod = "Stripe",
            string orderPrefix = "STRIPE")
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null) return null;

                if (await HasDateConflictAsync(request.RoomId, request.CheckInDate, request.CheckOutDate))
                    return null;

                var reservation = new Reservation
                {
                    RoomId = request.RoomId,
                    CustomerId = request.CustomerId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    Status = "PendingPayment",
                    CreatedAt = DateTime.Now
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                if (request.SelectedServiceIds.Any())
                {
                    var selectedServices = await _context.HotelServices
                        .Where(s => s.IsActive && request.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var service in selectedServices)
                    {
                        _context.ReservationServices.Add(new ReservationService
                        {
                            ReservationId = reservation.Id,
                            HotelServiceId = service.Id,
                            Quantity = 1,
                            UnitPrice = service.Price,
                            AddedAt = DateTime.Now
                        });
                    }
                }

                var orderId = $"{orderPrefix}_{reservation.Id}_{DateTime.Now:yyyyMMddHHmmss}";

                _context.Payments.Add(new Payment
                {
                    ReservationId = reservation.Id,
                    PaymentMethod = paymentMethod,
                    OrderId = orderId,
                    Amount = amount,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (reservation.Id, orderId);
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<bool> ConfirmPaymentAsync(string orderId, string transactionId)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null) return false;

            if (payment.Status == "Completed")
            {
                return true;
            }

            if (payment.Status != "Pending")
            {
                return false;
            }

            payment.Status = "Completed";
            payment.TransactionId = transactionId;
            payment.CompletedAt = DateTime.Now;

            payment.Reservation.Status = "Confirmed";

            var room = await _context.Rooms.FindAsync(payment.Reservation.RoomId);

            var existingCheckIn = await _context.CheckInOuts
                .FirstOrDefaultAsync(c => c.ReservationId == payment.Reservation.Id);

            if (existingCheckIn == null)
            {
                _context.CheckInOuts.Add(new CheckInOut
                {
                    ReservationId = payment.Reservation.Id,
                    CheckInTime = DateTime.Now,
                    TotalAmount = 0
                });
            }

            await _context.SaveChangesAsync();

            string roomNumber = room?.RoomNumber ?? payment.Reservation.RoomId.ToString();

            await _notificationService.CreateAndSendNotificationAsync(new Notification
            {
                Message = $"Đơn đặt phòng mới đã thanh toán {payment.PaymentMethod}: Phòng {roomNumber}",
                SenderName = "Hệ thống",
                SenderType = "System",
                RecipientType = "Admin",
                IsAnnouncement = true
            }, toAdminGroup: true);

            // Broadcast new reservation to admins
            await _reservationBroadcaster.BroadcastReservationCreatedAsync(
                payment.Reservation.Id,
                payment.Reservation.RoomId,
                roomNumber,
                payment.Reservation.Customer?.FullName ?? "Unknown",
                payment.Reservation.CheckInDate,
                payment.Reservation.CheckOutDate
            );

            return true;
        }

        public async Task<bool> FailPaymentAsync(string orderId)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "Pending");

            if (payment == null) return false;

            payment.Status = "Failed";
            payment.Reservation.Status = "Cancelled";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Reservation>> GetCustomerReservationsAsync(int customerId)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payments)
                .Include(r => r.ReservationServices)
                    .ThenInclude(rs => rs.HotelService)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> ProcessRefundAsync(
            int reservationId,
            int customerId,
            IStripeService stripeService)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.CustomerId == customerId);

            if (reservation == null)
                return (false, "Không tìm thấy đặt phòng.");

            if (reservation.Status != "Confirmed" && reservation.Status != "PendingPayment" && reservation.Status != "CheckedIn")
                return (false, "Không thể hoàn tiền cho đặt phòng này.");

            var refundDeadline = reservation.CheckInDate.AddHours(-48);
            if (DateTime.Now >= refundDeadline)
                return (false,
                    $"Không thể hoàn tiền. Yêu cầu hoàn tiền phải được thực hiện ít nhất 48 giờ trước khi nhận phòng " +
                    $"(hạn chót: {refundDeadline:dd/MM/yyyy HH:mm}).");

            var payment = reservation.Payments
                .FirstOrDefault(p => p.Status == "Completed" || p.Status == "Pending");

            if (payment == null)
                return (false, "Không tìm thấy thanh toán.");

            if (!string.Equals(payment.PaymentMethod, "Stripe", StringComparison.OrdinalIgnoreCase)
                || payment.Status != "Completed")
            {
                return (false, "Chỉ hỗ trợ hoàn tiền cho giao dịch Stripe đã thanh toán thành công.");
            }

            if (string.IsNullOrWhiteSpace(payment.TransactionId))
                return (false, "Không tìm thấy mã giao dịch Stripe để hoàn tiền.");

            var refundResult = await stripeService.RefundAsync(
                payment.TransactionId,
                (long)payment.Amount,
                $"Hoan tien dat phong #{reservation.Id}");

            if (refundResult == null || !string.Equals(refundResult.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Hoàn tiền Stripe thất bại. Vui lòng thử lại sau.");
            }

            payment.RefundTransactionId = refundResult.Id;
            payment.Status = "Refunded";
            payment.RefundedAt = DateTime.Now;
            reservation.Status = "Cancelled";

            await _context.SaveChangesAsync();
            return (true, "Hoàn tiền thành công! Đặt phòng đã được hủy.");
        }

        private async Task<bool> HasDateConflictAsync(
            int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
        {
            return await _context.Reservations
                .Where(r => r.RoomId == roomId
                    && (r.Status == "Confirmed" || r.Status == "PendingPayment" || r.Status == "CheckedIn")
                    && (excludeReservationId == null || r.Id != excludeReservationId)
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn)
                .AnyAsync();
        }

        public async Task<List<(DateTime CheckIn, DateTime CheckOut)>> GetReservedPeriodsAsync(
            int roomId, int? excludeReservationId = null)
        {
            return await _context.Reservations
                .Where(r => r.RoomId == roomId
                    && (r.Status == "Confirmed" || r.Status == "PendingPayment" || r.Status == "CheckedIn")
                    && (excludeReservationId == null || r.Id != excludeReservationId)
                    && r.CheckOutDate >= DateTime.Today)
                .OrderBy(r => r.CheckInDate)
                .Select(r => ValueTuple.Create(r.CheckInDate, r.CheckOutDate))
                .ToListAsync();
        }

        private async Task<Dictionary<int, decimal>> GetServicePricesAsync(List<int> serviceIds)
        {
            if (!serviceIds.Any()) return new Dictionary<int, decimal>();
            return await _context.HotelServices
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Price);
        }

        private static decimal CalculateTotal(
            Room room, BookingRequest request, Dictionary<int, decimal> servicePrices)
        {
            return room.Price; // deposit: 1 night only
        }
    }
}
