using MTOGO.Services.PaymentAPI.Models.Dto;

namespace MTOGO.Services.PaymentAPI.Services.IServices
{
    public interface IPaymentService
    {
        Task ProcessPayment(PaymentRequestDto paymentRequest);
    }
}
