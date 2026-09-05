using Sheard.Type;

namespace Domain.Payments;

public interface IPaymentService
{
    /// <summary>
    /// Opens a fresh attempt to settle an order, for the whole of what is owed. Any attempt
    /// still outstanding is written off first, so an order never has two live at once.
    /// </summary>
    Task<Payment> Record(RecordPaymentParams recordPaymentParams);

    Task<Payment?> FindById(Guid id);
    Task<List<Payment>> FindByOrderId(Guid orderId);
    Task<PaginationResult<Payment>> Search(PaymentFilters paymentFilters);

    /// <summary>
    /// Settles, fails or refunds an attempt. This is where a provider's answer lands.
    /// </summary>
    Task<Payment> UpdateStatus(UpdatePaymentStatusParams updatePaymentStatusParams);

    /// <summary>
    /// Settles the cash the courier collected on handover. Called when an order is marked
    /// delivered, and silent when there is nothing owed in cash.
    /// </summary>
    Task SettleCashOnDelivery(Guid orderId, string updatedById);

    /// <summary>
    /// Writes off whatever is still outstanding on an order that has been called off. Money
    /// already taken is left alone: returning it is a refund, and a decision for staff.
    /// </summary>
    Task VoidOutstanding(Guid orderId, string updatedById);
}
