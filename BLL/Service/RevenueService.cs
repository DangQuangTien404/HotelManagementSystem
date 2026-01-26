using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class RevenueService : IRevenueService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IGenericRepository<CheckInOut> _checkInOutRepository;
        private readonly IGenericRepository<Room> _roomRepository;

        public RevenueService(
            IReservationRepository reservationRepository,
            IGenericRepository<CheckInOut> checkInOutRepository,
            IGenericRepository<Room> roomRepository)
        {
            _reservationRepository = reservationRepository;
            _checkInOutRepository = checkInOutRepository;
            _roomRepository = roomRepository;
        }

        public async Task<RevenueDto> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, string periodType)
        {
            var checkIns = await _checkInOutRepository.GetAllAsync();
            var checkInsList = checkIns.Cast<CheckInOut>().ToList();

            var revenue = checkInsList
                .Where(c => c.CheckOutTime.HasValue &&
                           c.CheckOutTime.Value.Date >= startDate.Date &&
                           c.CheckOutTime.Value.Date <= endDate.Date)
                .Sum(c => (decimal?)c.TotalAmount) ?? 0;

            var totalCost = await GetTotalCostAsync(startDate, endDate);
            var profit = revenue - totalCost;

            return new RevenueDto
            {
                TotalRevenue = revenue,
                TotalCost = totalCost,
                Profit = profit,
                StartDate = startDate,
                EndDate = endDate,
                PeriodType = periodType
            };
        }

        public async Task<List<RevenueByRoomTypeDto>> GetRevenueByRoomTypeAsync(DateTime startDate, DateTime endDate)
        {
            var reservations = await _reservationRepository.GetAllWithDetailsAsync();
            var checkIns = await _checkInOutRepository.GetAllAsync();
            var checkInsList = checkIns.Cast<CheckInOut>().ToList();

            // Get reservations with checkouts in the date range
            var reservationsInRange = reservations
                .Where(r => r.CheckInOuts != null && r.CheckInOuts.Any(c => 
                    c.CheckOutTime.HasValue &&
                    c.CheckOutTime.Value.Date >= startDate.Date &&
                    c.CheckOutTime.Value.Date <= endDate.Date))
                .ToList();

            // Group by room type and calculate revenue
            var revenueByRoomType = reservationsInRange
                .GroupBy(r => r.Room?.RoomType.ToString() ?? "Unknown")
                .Select(g => 
                {
                    var reservationIds = g.Select(r => r.Id).ToList();
                    var totalRevenue = checkInsList
                        .Where(c => reservationIds.Contains(c.ReservationId) &&
                                   c.CheckOutTime.HasValue &&
                                   c.CheckOutTime.Value.Date >= startDate.Date &&
                                   c.CheckOutTime.Value.Date <= endDate.Date)
                        .Sum(c => (decimal?)c.TotalAmount) ?? 0;
                    
                    return new RevenueByRoomTypeDto
                    {
                        RoomType = g.Key,
                        BookingCount = g.Count(),
                        TotalRevenue = totalRevenue,
                        AverageRevenue = g.Count() > 0 ? totalRevenue / g.Count() : 0
                    };
                })
                .ToList();

            return revenueByRoomType;
        }

        public async Task<decimal> GetTotalCostAsync(DateTime startDate, DateTime endDate)
        {
            // Simple cost calculation: Assume 30% of revenue is cost
            // In real application, this would come from actual cost data
            var checkIns = await _checkInOutRepository.GetAllAsync();
            var checkInsList = checkIns.Cast<CheckInOut>().ToList();

            var revenue = checkInsList
                .Where(c => c.CheckOutTime.HasValue &&
                           c.CheckOutTime.Value.Date >= startDate.Date &&
                           c.CheckOutTime.Value.Date <= endDate.Date)
                .Sum(c => (decimal?)c.TotalAmount) ?? 0;

            return revenue * 0.3m; // 30% cost assumption
        }

        public async Task<decimal> GetProfitAsync(DateTime startDate, DateTime endDate)
        {
            var revenue = await GetRevenueByPeriodAsync(startDate, endDate, "Custom");
            return revenue.Profit;
        }

        public async Task<List<PaymentMethodStatsDto>> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate)
        {
            var checkIns = await _checkInOutRepository.GetAllAsync();
            var checkInsList = checkIns.Cast<CheckInOut>().ToList();

            var filteredCheckIns = checkInsList
                .Where(c => c.CheckOutTime.HasValue &&
                           c.CheckOutTime.Value.Date >= startDate.Date &&
                           c.CheckOutTime.Value.Date <= endDate.Date)
                .ToList();

            var totalAmount = filteredCheckIns.Sum(c => (decimal?)c.TotalAmount) ?? 0;
            var totalCount = filteredCheckIns.Count;

            if (totalCount == 0 || totalAmount == 0)
            {
                return new List<PaymentMethodStatsDto>();
            }

            // Since we don't have PaymentMethod in CheckInOut, we'll simulate with common methods
            // In real application, this would come from actual payment method data
            var stats = new List<PaymentMethodStatsDto>
            {
                new PaymentMethodStatsDto
                {
                    PaymentMethod = "Cash",
                    TransactionCount = Math.Max(1, totalCount / 3),
                    TotalAmount = totalAmount * 0.3m,
                    Percentage = 30
                },
                new PaymentMethodStatsDto
                {
                    PaymentMethod = "Credit Card",
                    TransactionCount = Math.Max(1, totalCount / 2),
                    TotalAmount = totalAmount * 0.5m,
                    Percentage = 50
                },
                new PaymentMethodStatsDto
                {
                    PaymentMethod = "Bank Transfer",
                    TransactionCount = Math.Max(1, totalCount / 6),
                    TotalAmount = totalAmount * 0.2m,
                    Percentage = 20
                }
            };

            return stats.Where(s => s.TransactionCount > 0 && s.TotalAmount > 0).ToList();
        }

        public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate, string periodType)
        {
            // Default to current month if not specified
            var start = startDate ?? DateTime.Now.Date.AddDays(-DateTime.Now.Day + 1);
            var end = endDate ?? DateTime.Now.Date;

            // Adjust dates based on period type
            switch (periodType?.ToLower())
            {
                case "day":
                    start = DateTime.Now.Date;
                    end = DateTime.Now.Date;
                    break;
                case "week":
                    start = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
                    end = start.AddDays(6);
                    break;
                case "month":
                    start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    end = start.AddMonths(1).AddDays(-1);
                    break;
                case "year":
                    start = new DateTime(DateTime.Now.Year, 1, 1);
                    end = new DateTime(DateTime.Now.Year, 12, 31);
                    break;
            }

            var revenue = await GetRevenueByPeriodAsync(start, end, periodType ?? "Custom");
            var revenueByRoomType = await GetRevenueByRoomTypeAsync(start, end);
            var paymentMethodStats = await GetPaymentMethodStatsAsync(start, end);

            return new RevenueReportDto
            {
                Revenue = revenue,
                RevenueByRoomType = revenueByRoomType,
                PaymentMethodStats = paymentMethodStats
            };
        }
    }
}

