using Microsoft.AspNetCore.Http;

namespace HotelManagementSystem.Business.interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(string orderId, string orderInfo, long amount, string returnUrl, string ipAddress);
        bool VerifySignature(IQueryCollection query);
    }
}
