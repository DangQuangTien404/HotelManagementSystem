using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using HotelManagementSystem.Business.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HotelManagementSystem.Business.service
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(string orderId, string orderInfo, long amount, string returnUrl, string ipAddress)
        {
            var baseUrl = _configuration["VNPay:BaseUrl"]!;
            var tmnCode = _configuration["VNPay:TmnCode"]!;
            var version = _configuration["VNPay:Version"] ?? "2.1.0";
            var command = _configuration["VNPay:Command"] ?? "pay";
            var currCode = _configuration["VNPay:CurrCode"] ?? "VND";
            var locale = _configuration["VNPay:Locale"] ?? "vn";
            var orderType = _configuration["VNPay:OrderType"] ?? "other";

            var requestData = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = version,
                ["vnp_Command"] = command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = (amount * 100).ToString(CultureInfo.InvariantCulture),
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = currCode,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_Locale"] = locale,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = orderType,
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_TxnRef"] = orderId,
                ["vnp_ExpireDate"] = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss")
            };

            var hashData = BuildQueryString(requestData, encode: true);
            var queryString = BuildQueryString(requestData, encode: true);

            var secureHash = ComputeHmacSha512(_configuration["VNPay:HashSecret"]!, hashData);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public bool VerifySignature(IQueryCollection query)
        {
            var secureHash = query["vnp_SecureHash"].ToString();
            if (string.IsNullOrWhiteSpace(secureHash))
            {
                return false;
            }

            var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var key in query.Keys)
            {
                if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = query[key].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    data[key] = value;
                }
            }

            var hashData = BuildQueryString(data, encode: true);
            var computed = ComputeHmacSha512(_configuration["VNPay:HashSecret"]!, hashData);

            return computed.Equals(secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQueryString(SortedDictionary<string, string> data, bool encode)
        {
            return string.Join("&", data.Select(x =>
            {
                var key = encode ? WebUtility.UrlEncode(x.Key) : x.Key;
                var value = encode ? WebUtility.UrlEncode(x.Value) : x.Value;
                return $"{key}={value}";
            }));
        }

        private static string ComputeHmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
