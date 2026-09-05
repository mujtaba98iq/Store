namespace Domain.Payments;

public interface IPaymentsRepository
{
    Task<Payment> Create(Payment payment);
    Task<Payment> Update(Payment payment);
    Task<Payment?> FindById(Guid id);

    /// <summary>
    /// Used to spot a provider callback that has already been banked, so the same money is
    /// not recorded twice.
    /// </summary>
    Task<Payment?> FindByTransactionId(string transactionId);

    /// <summary>
    /// Every attempt against one order, oldest first: the history of how it was paid.
    /// </summary>
    Task<List<Payment>> FindByOrderId(Guid orderId);

    Task<List<Payment>> FindByFilters(PaymentFilters paymentFilters);
    Task<int> GetTotalCountByFilters(PaymentFilters paymentFilters);
}
