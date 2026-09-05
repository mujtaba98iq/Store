using Domain.Payments;

namespace RestApi.Payments;

public class PaymentResponseFormatter : IPaymentResponseFormatter
{
    public PaymentListResponse Many(IEnumerable<Payment> payments, int totalCount)
    {
        return new PaymentListResponse
        {
            Data = payments.Select(One).ToList(),
            TotalCount = totalCount
        };
    }

    public PaymentResponse One(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id.ToString(),
            OrderId = payment.OrderId.ToString(),
            PaymentMethod = payment.PaymentMethod.ToString(),
            PaymentStatus = payment.PaymentStatus.ToString(),
            Amount = payment.Amount,
            TransactionId = payment.TransactionId,
            PaidAt = payment.PaidAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
            CreatedById = payment.CreatedById,
            UpdatedById = payment.UpdatedById
        };
    }
}
