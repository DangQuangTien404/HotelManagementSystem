using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace HotelManagementSystem.Business.service
{
    public class MoMoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MoMoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<MoMoCreatePaymentResponse?> CreatePaymentAsync(
            string orderId, string orderInfo, long amount, string redirectUrl, string ipnUrl)
        {
            var partnerCode = _configuration["MoMo:PartnerCode"]!;
            var accessKey = _configuration["MoMo:AccessKey"]!;
            var secretKey = _configuration["MoMo:SecretKey"]!;
            var endpoint = _configuration["MoMo:PaymentUrl"]!;

            var requestId = orderId;
            var requestType = "payWithMethod";
            var extraData = "";

            var rawSignature =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={ipnUrl}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={partnerCode}" +
                $"&redirectUrl={redirectUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={requestType}";

            var signature = ComputeHmacSha256(rawSignature, secretKey);

            var requestBody = new
            {
                partnerCode,
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                requestType,
                extraData,
                lang = "vi",
                signature
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<MoMoCreatePaymentResponse>(
                responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public bool VerifySignature(MoMoCallbackData data)
        {
            var accessKey = _configuration["MoMo:AccessKey"]!;
            var secretKey = _configuration["MoMo:SecretKey"]!;

            var rawSignature =
                $"accessKey={accessKey}" +
                $"&amount={data.Amount}" +
                $"&extraData={data.ExtraData}" +
                $"&message={data.Message}" +
                $"&orderId={data.OrderId}" +
                $"&orderInfo={data.OrderInfo}" +
                $"&orderType={data.OrderType}" +
                $"&partnerCode={data.PartnerCode}" +
                $"&payType={data.PayType}" +
                $"&requestId={data.RequestId}" +
                $"&responseTime={data.ResponseTime}" +
                $"&resultCode={data.ResultCode}" +
                $"&transId={data.TransId}";

            var computedSignature = ComputeHmacSha256(rawSignature, secretKey);
            return computedSignature == data.Signature;
        }

        public async Task<MoMoRefundResponse?> RefundAsync(
            string orderId, long transId, long amount, string description)
        {
            var partnerCode = _configuration["MoMo:PartnerCode"]!;
            var accessKey = _configuration["MoMo:AccessKey"]!;
            var secretKey = _configuration["MoMo:SecretKey"]!;
            var endpoint = _configuration["MoMo:RefundUrl"]!;

            var requestId = Guid.NewGuid().ToString();

            var rawSignature =
                $"accessKey={accessKey}" +
                $"&amount={amount}" +
                $"&description={description}" +
                $"&orderId={orderId}" +
                $"&partnerCode={partnerCode}" +
                $"&requestId={requestId}" +
                $"&transId={transId}";

            var signature = ComputeHmacSha256(rawSignature, secretKey);

            var requestBody = new
            {
                partnerCode,
                orderId,
                requestId,
                amount,
                transId,
                lang = "vi",
                description,
                signature
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<MoMoRefundResponse>(
                responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public class MoMoCreatePaymentResponse
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string PayUrl { get; set; } = string.Empty;
    }

    public class MoMoCallbackData
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class MoMoRefundResponse
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
    }
}
