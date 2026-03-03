using System;

namespace HotelManagementSystem.Data.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string? RequestId { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? RefundTransactionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;
}
