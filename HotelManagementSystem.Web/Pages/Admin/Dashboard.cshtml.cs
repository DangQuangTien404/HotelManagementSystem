using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly HotelManagementDbContext _context;

        // ── Filter ────────────────────────────────────────────────────────────
        [BindProperty(SupportsGet = true)]
        public string Period { get; set; } = "month";

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public DateTime RangeFrom { get; private set; }
        public DateTime RangeTo   { get; private set; }
        public string   RangeLabel { get; private set; } = string.Empty;

        // ── KPI cards ─────────────────────────────────────────────────────────
        public decimal TotalRevenue       { get; set; }
        public decimal MoMoRevenue        { get; set; }
        public decimal VietQrRevenue      { get; set; }
        public decimal TotalRefundAmount  { get; set; }

        public int TotalReservations     { get; set; }
        public int ConfirmedReservations { get; set; }
        public int CancelledReservations { get; set; }
        public int PendingReservations   { get; set; }
        public int ActiveGuests          { get; set; }

        public int TotalRefunds          { get; set; }

        // ── Chart payloads (JSON strings embedded in the page) ────────────────
        public string RevenueTrendJson      { get; set; } = "{}";
        public string PaymentMethodJson     { get; set; } = "{}";
        public string ReservationStatusJson { get; set; } = "{}";

        // ── Detail tables ─────────────────────────────────────────────────────
        public List<Payment> RecentPayments { get; set; } = new();
        public List<Payment> RecentRefunds  { get; set; } = new();

        public DashboardModel(HotelManagementDbContext context) => _context = context;

        // ─────────────────────────────────────────────────────────────────────
        public async Task OnGetAsync()
        {
            ResolveRange();

            // ── Payments ─────────────────────────────────────────────────────
            var payments = await _context.Payments
                .Include(p => p.Reservation).ThenInclude(r => r.Room)
                .Include(p => p.Reservation).ThenInclude(r => r.Customer)
                .Where(p => p.CreatedAt >= RangeFrom && p.CreatedAt <= RangeTo)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var completed = payments.Where(p => p.Status == "Completed").ToList();
            var refunded  = payments.Where(p => p.Status == "Refunded").ToList();

            TotalRevenue      = completed.Sum(p => p.Amount);
            MoMoRevenue       = completed.Where(p => p.PaymentMethod == "MoMo").Sum(p => p.Amount);
            VietQrRevenue     = completed.Where(p => p.PaymentMethod == "VietQR").Sum(p => p.Amount);
            TotalRefunds      = refunded.Count;
            TotalRefundAmount = refunded.Sum(p => p.Amount);

            RecentPayments = completed.Take(10).ToList();
            RecentRefunds  = refunded.Take(10).ToList();

            // ── Reservations ─────────────────────────────────────────────────
            var reservations = await _context.Reservations
                .Where(r => r.CreatedAt >= RangeFrom && r.CreatedAt <= RangeTo)
                .ToListAsync();

            TotalReservations     = reservations.Count;
            ConfirmedReservations = reservations.Count(r =>
                r.Status is "Confirmed" or "CheckedIn" or "Completed" or "CheckedOut");
            CancelledReservations = reservations.Count(r => r.Status == "Cancelled");
            PendingReservations   = reservations.Count(r => r.Status == "PendingPayment");

            ActiveGuests = await _context.Reservations.CountAsync(r => r.Status == "CheckedIn");

            // ── Charts ───────────────────────────────────────────────────────
            BuildRevenueTrend(completed);
            BuildPaymentMethodChart(completed);
            BuildReservationStatusChart(reservations);
        }

        // ─────────────────────────────────────────────────────────────────────
        private void ResolveRange()
        {
            var now = DateTime.Now;
            switch (Period)
            {
                case "today":
                    RangeFrom  = now.Date;
                    RangeTo    = now.Date.AddDays(1).AddTicks(-1);
                    RangeLabel = now.ToString("dd/MM/yyyy");
                    break;

                case "week":
                    var monday = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
                    RangeFrom  = monday;
                    RangeTo    = monday.AddDays(7).AddTicks(-1);
                    RangeLabel = $"{monday:dd/MM} – {monday.AddDays(6):dd/MM/yyyy}";
                    break;

                case "year":
                    RangeFrom  = new DateTime(now.Year, 1, 1);
                    RangeTo    = new DateTime(now.Year, 12, 31, 23, 59, 59);
                    RangeLabel = now.Year.ToString();
                    break;

                case "custom" when From.HasValue && To.HasValue:
                    RangeFrom  = From.Value.Date;
                    RangeTo    = To.Value.Date.AddDays(1).AddTicks(-1);
                    RangeLabel = $"{From.Value:dd/MM/yyyy} – {To.Value:dd/MM/yyyy}";
                    break;

                default: // month
                    Period     = "month";
                    RangeFrom  = new DateTime(now.Year, now.Month, 1);
                    RangeTo    = RangeFrom.AddMonths(1).AddTicks(-1);
                    RangeLabel = now.ToString("MM/yyyy");
                    break;
            }
        }

        private void BuildRevenueTrend(List<Payment> completed)
        {
            var totalDays = (int)(RangeTo - RangeFrom).TotalDays + 1;

            List<string>  labels;
            List<decimal> values;

            if (Period == "year")
            {
                var year = RangeFrom.Year;
                labels = Enumerable.Range(1, 12)
                    .Select(m => new DateTime(year, m, 1).ToString("MMM"))
                    .ToList();
                values = Enumerable.Range(1, 12)
                    .Select(m => completed
                        .Where(p => (p.CompletedAt ?? p.CreatedAt).Month == m
                                 && (p.CompletedAt ?? p.CreatedAt).Year  == year)
                        .Sum(p => p.Amount))
                    .ToList();
            }
            else if (totalDays > 31)
            {
                // Group by week
                labels = new List<string>();
                values = new List<decimal>();
                var cursor = RangeFrom;
                while (cursor <= RangeTo)
                {
                    var end = cursor.AddDays(7);
                    labels.Add(cursor.ToString("dd/MM"));
                    values.Add(completed
                        .Where(p => (p.CompletedAt ?? p.CreatedAt) >= cursor
                                 && (p.CompletedAt ?? p.CreatedAt) < end)
                        .Sum(p => p.Amount));
                    cursor = end;
                }
            }
            else
            {
                // Group by day
                labels = Enumerable.Range(0, totalDays)
                    .Select(i => RangeFrom.AddDays(i).ToString("dd/MM"))
                    .ToList();
                values = Enumerable.Range(0, totalDays)
                    .Select(i =>
                    {
                        var day = RangeFrom.AddDays(i).Date;
                        return completed
                            .Where(p => (p.CompletedAt ?? p.CreatedAt).Date == day)
                            .Sum(p => p.Amount);
                    })
                    .ToList();
            }

            RevenueTrendJson = JsonSerializer.Serialize(new { labels, values });
        }

        private void BuildPaymentMethodChart(List<Payment> completed)
        {
            PaymentMethodJson = JsonSerializer.Serialize(new
            {
                labels = new[] { "MoMo", "VietQR" },
                values = new[]
                {
                    completed.Where(p => p.PaymentMethod == "MoMo").Sum(p => p.Amount),
                    completed.Where(p => p.PaymentMethod == "VietQR").Sum(p => p.Amount)
                }
            });
        }

        private void BuildReservationStatusChart(List<Reservation> reservations)
        {
            ReservationStatusJson = JsonSerializer.Serialize(new
            {
                labels = new[] { "Xác nhận", "Đã nhận phòng", "Hoàn thành", "Đã hủy", "Chờ TT" },
                values = new[]
                {
                    reservations.Count(r => r.Status == "Confirmed"),
                    reservations.Count(r => r.Status == "CheckedIn"),
                    reservations.Count(r => r.Status is "Completed" or "CheckedOut"),
                    reservations.Count(r => r.Status == "Cancelled"),
                    reservations.Count(r => r.Status == "PendingPayment")
                }
            });
        }
    }
}
