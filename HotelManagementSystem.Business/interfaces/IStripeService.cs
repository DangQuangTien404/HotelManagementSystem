using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IStripeService
    {
        Task<StripeCheckoutSessionResponse?> CreateCheckoutSessionAsync(
            string orderId,
            string orderInfo,
            long amount,
            string successUrl,
            string cancelUrl);

        Task<StripeSessionDetails?> GetSessionAsync(string sessionId);

        Task<StripeRefundResponse?> RefundAsync(string paymentIntentId, long amount, string reason);

        StripeWebhookEvent? ParseWebhookEvent(string payload, string signatureHeader);
    }

    public class StripeCheckoutSessionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class StripeSessionDetails
    {
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; } = string.Empty;

        [JsonPropertyName("payment_intent")]
        public string? PaymentIntent { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class StripeRefundResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class StripeWebhookEvent
    {
        public string Type { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public string? SessionId { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
