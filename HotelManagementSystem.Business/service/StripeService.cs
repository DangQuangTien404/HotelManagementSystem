using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HotelManagementSystem.Business.interfaces;
using Microsoft.Extensions.Configuration;

namespace HotelManagementSystem.Business.service
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public StripeService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<StripeCheckoutSessionResponse?> CreateCheckoutSessionAsync(
            string orderId,
            string orderInfo,
            long amount,
            string successUrl,
            string cancelUrl)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            var payload = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = successUrl,
                ["cancel_url"] = cancelUrl,
                ["line_items[0][price_data][currency]"] = "vnd",
                ["line_items[0][price_data][unit_amount]"] = amount.ToString(),
                ["line_items[0][price_data][product_data][name]"] = orderInfo,
                ["line_items[0][quantity]"] = "1",
                ["client_reference_id"] = orderId,
                ["metadata[order_id]"] = orderId
            };
            request.Content = new FormUrlEncodedContent(payload);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<StripeCheckoutSessionResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return session;
        }

        public async Task<StripeSessionDetails?> GetSessionAsync(string sessionId)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.stripe.com/v1/checkout/sessions/{sessionId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StripeSessionDetails>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<StripeRefundResponse?> RefundAsync(string paymentIntentId, long amount, string reason)
        {
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey)) return null;

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/refunds");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

            var payload = new Dictionary<string, string>
            {
                ["payment_intent"] = paymentIntentId,
                ["amount"] = amount.ToString(),
                ["reason"] = "requested_by_customer",
                ["metadata[description]"] = reason
            };
            request.Content = new FormUrlEncodedContent(payload);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StripeRefundResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public StripeWebhookEvent? ParseWebhookEvent(string payload, string signatureHeader)
        {
            if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signatureHeader))
            {
                return null;
            }

            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(webhookSecret) || webhookSecret == "YOUR_STRIPE_WEBHOOK_SECRET")
            {
                return null;
            }

            if (!VerifyWebhookSignature(payload, signatureHeader, webhookSecret))
            {
                return null;
            }

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (!root.TryGetProperty("type", out var typeElement))
            {
                return null;
            }

            if (!root.TryGetProperty("data", out var dataElement)
                || !dataElement.TryGetProperty("object", out var objectElement))
            {
                return null;
            }

            var orderId = objectElement.TryGetProperty("client_reference_id", out var clientRef)
                ? clientRef.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(orderId)
                && objectElement.TryGetProperty("metadata", out var metadataElement)
                && metadataElement.ValueKind == JsonValueKind.Object
                && metadataElement.TryGetProperty("order_id", out var metadataOrderId))
            {
                orderId = metadataOrderId.GetString();
            }

            return new StripeWebhookEvent
            {
                Type = typeElement.GetString() ?? string.Empty,
                OrderId = orderId,
                SessionId = objectElement.TryGetProperty("id", out var sessionIdElement)
                    ? sessionIdElement.GetString()
                    : null,
                TransactionId = objectElement.TryGetProperty("payment_intent", out var paymentIntentElement)
                    ? paymentIntentElement.GetString()
                    : null,
                PaymentStatus = objectElement.TryGetProperty("payment_status", out var paymentStatusElement)
                    ? paymentStatusElement.GetString()
                    : null
            };
        }

        private static bool VerifyWebhookSignature(string payload, string signatureHeader, string webhookSecret)
        {
            var timestamp = string.Empty;
            var signatures = new List<string>();

            foreach (var part in signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
                if (pair.Length != 2) continue;

                if (pair[0] == "t") timestamp = pair[1];
                if (pair[0] == "v1") signatures.Add(pair[1]);
            }

            if (string.IsNullOrWhiteSpace(timestamp) || signatures.Count == 0)
            {
                return false;
            }

            if (!long.TryParse(timestamp, out var unixTime))
            {
                return false;
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            if (Math.Abs((DateTimeOffset.UtcNow - issuedAt).TotalMinutes) > 5)
            {
                return false;
            }

            var signedPayload = $"{timestamp}.{payload}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return signatures.Any(sig => SecureEquals(sig, computedSignature));
        }

        private static bool SecureEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);

            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
    }
}
