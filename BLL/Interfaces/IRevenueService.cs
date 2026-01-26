using DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRevenueService
    {
        Task<RevenueDto> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, string periodType);
        Task<List<RevenueByRoomTypeDto>> GetRevenueByRoomTypeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalCostAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetProfitAsync(DateTime startDate, DateTime endDate);
        Task<List<PaymentMethodStatsDto>> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate);
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime? startDate, DateTime? endDate, string periodType);
    }
}

