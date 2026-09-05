using Domain.Payments;

namespace RestApi.Payments;

public interface IPaymentResponseFormatter
{
    PaymentResponse One(Payment payment);
    PaymentListResponse Many(IEnumerable<Payment> payments, int totalCount);
}
