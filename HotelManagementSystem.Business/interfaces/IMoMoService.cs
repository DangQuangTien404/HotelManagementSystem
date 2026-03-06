using HotelManagementSystem.Business.service;
using System.Threading.Tasks;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IMoMoService
    {
        Task<MoMoCreatePaymentResponse?> CreatePaymentAsync(string orderId, string orderInfo, long amount, string redirectUrl, string ipnUrl);
        bool VerifySignature(MoMoCallbackData data);
        Task<MoMoRefundResponse?> RefundAsync(string orderId, long transId, long amount, string description);
    }
}
