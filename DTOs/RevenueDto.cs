using System;
using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class RevenueDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PeriodType { get; set; } = string.Empty; // Day, Week, Month, Year
    }

    public class RevenueByRoomTypeDto
    {
        public string RoomType { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenue { get; set; }
    }

    public class PaymentMethodStatsDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RevenueReportDto
    {
        public RevenueDto Revenue { get; set; } = new RevenueDto();
        public List<RevenueByRoomTypeDto> RevenueByRoomType { get; set; } = new List<RevenueByRoomTypeDto>();
        public List<PaymentMethodStatsDto> PaymentMethodStats { get; set; } = new List<PaymentMethodStatsDto>();
    }
}

