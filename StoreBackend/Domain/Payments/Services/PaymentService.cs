using Domain.Exeptions;
using Domain.Orders;
using Sheard.Type;

namespace Domain.Payments
{
    public class PaymentService(
        IPaymentsRepository paymentsRepository,
        IOrdersRepository ordersRepository) : IPaymentService
    {
        /// <summary>
        /// Where a payment may go next. A failed attempt is a dead end on purpose: a customer
        /// trying again gets a new row, so the history keeps both the decline and the retry
        /// rather than overwriting one with the other.
        /// </summary>
        private static readonly Dictionary<PaymentStatus, PaymentStatus[]> AllowedTransitions = new()
        {
            [PaymentStatus.Pending] = [PaymentStatus.Paid, PaymentStatus.Failed],
            [PaymentStatus.Paid] = [PaymentStatus.Refunded],
            [PaymentStatus.Failed] = [],
            [PaymentStatus.Refunded] = [],
        };

        public async Task<Payment> Record(RecordPaymentParams recordPaymentParams)
        {
            var order = await ordersRepository.FindById(recordPaymentParams.OrderId)
                        ?? throw new ResourceNotFoundException("Order", $"Order with ID {recordPaymentParams.OrderId} not found");

            EnsureOrderCanTakeMoney(order);

            var payments = await paymentsRepository.FindByOrderId(order.Id);

            if (payments.Any(payment => payment.PaymentStatus == PaymentStatus.Paid))
            {
                throw new OrderAlreadyPaidException($"Order {order.OrderNumber} has already been paid.");
            }

            var recordedAt = DateTime.UtcNow;

            // Whatever is still outstanding is written off before the new attempt opens.
            // Leaving both live would let a late callback on the abandoned one settle an
            // order the customer has since paid for by other means.
            foreach (var outstanding in payments.Where(payment => payment.PaymentStatus == PaymentStatus.Pending))
            {
                await Transition(outstanding, PaymentStatus.Failed, recordPaymentParams.CreatedById, outstanding.TransactionId);
            }

            // The amount comes off the order, not off the request: what is owed is not
            // something the caller gets to name. Part payments would change that, and would
            // need an answer for what the outstanding balance is afterwards.
            var payment = await paymentsRepository.Create(new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethod = recordPaymentParams.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                Amount = order.TotalAmount,
                CreatedAt = recordedAt,
                CreatedById = recordPaymentParams.CreatedById
            });

            return await paymentsRepository.FindById(payment.Id) ?? payment;
        }

        public async Task<Payment?> FindById(Guid id)
        {
            return await paymentsRepository.FindById(id);
        }

        public async Task<List<Payment>> FindByOrderId(Guid orderId)
        {
            return await paymentsRepository.FindByOrderId(orderId);
        }

        public async Task<PaginationResult<Payment>> Search(PaymentFilters paymentFilters)
        {
            var payments = await paymentsRepository.FindByFilters(paymentFilters);
            var totalCount = await paymentsRepository.GetTotalCountByFilters(paymentFilters);

            return new PaginationResult<Payment>
            {
                TotalCount = totalCount,
                Data = payments
            };
        }

        public async Task<Payment> UpdateStatus(UpdatePaymentStatusParams updatePaymentStatusParams)
        {
            var payment = await paymentsRepository.FindById(updatePaymentStatusParams.PaymentId)
                          ?? throw new ResourceNotFoundException("Payment", $"Payment with ID {updatePaymentStatusParams.PaymentId} not found");

            var status = updatePaymentStatusParams.PaymentStatus;

            EnsureTransitionIsAllowed(payment.PaymentStatus, status);

            if (status == PaymentStatus.Paid)
            {
                await EnsureTransactionReference(payment, updatePaymentStatusParams.TransactionId);
            }

            return await Transition(payment, status, updatePaymentStatusParams.UpdatedById, updatePaymentStatusParams.TransactionId);
        }

        public async Task SettleCashOnDelivery(Guid orderId, string updatedById)
        {
            // Only cash is settled here. An online payment still pending when the parcel
            // arrives means the money never actually came through, which is something for
            // somebody to look at rather than something to quietly mark as paid.
            var outstanding = (await paymentsRepository.FindByOrderId(orderId))
                .Where(payment => payment.PaymentStatus == PaymentStatus.Pending)
                .Where(payment => payment.PaymentMethod == PaymentMethod.CashOnDelivery)
                .ToList();

            foreach (var payment in outstanding)
            {
                await Transition(payment, PaymentStatus.Paid, updatedById, payment.TransactionId);
            }
        }

        public async Task VoidOutstanding(Guid orderId, string updatedById)
        {
            var outstanding = (await paymentsRepository.FindByOrderId(orderId))
                .Where(payment => payment.PaymentStatus == PaymentStatus.Pending)
                .ToList();

            foreach (var payment in outstanding)
            {
                await Transition(payment, PaymentStatus.Failed, updatedById, payment.TransactionId);
            }
        }

        private async Task<Payment> Transition(Payment payment, PaymentStatus status, string updatedById, string? transactionId)
        {
            var now = DateTime.UtcNow;

            payment.PaymentStatus = status;
            payment.TransactionId = transactionId ?? payment.TransactionId;
            payment.UpdatedAt = now;
            payment.UpdatedById = updatedById;

            // Stamped once, when the money first lands. A refund leaves it alone: it says
            // when the payment arrived, not whether the shop still holds it.
            if (status == PaymentStatus.Paid)
            {
                payment.PaidAt ??= now;
            }

            await paymentsRepository.Update(payment);

            return await paymentsRepository.FindById(payment.Id) ?? payment;
        }

        private static void EnsureOrderCanTakeMoney(Order order)
        {
            if (order.Status == OrderStatus.Cancelled)
            {
                throw new OrderNotPayableException($"Order {order.OrderNumber} has been cancelled and cannot be paid for.");
            }
        }

        /// <summary>
        /// A settled payment has to say where the money came from, so it can be matched
        /// against the provider's own records when somebody disputes it. Cash is the
        /// exception: there is no provider, and the courier is the receipt.
        /// </summary>
        private async Task EnsureTransactionReference(Payment payment, string? transactionId)
        {
            if (payment.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                return;
            }

            var reference = transactionId ?? payment.TransactionId;

            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new MissingTransactionReferenceException(
                    $"A {payment.PaymentMethod} payment needs a TransactionId before it can be marked as paid.");
            }

            // Caught here rather than left to the unique index, so a callback that arrives
            // twice reads as the duplicate it is instead of as a database error.
            var existing = await paymentsRepository.FindByTransactionId(reference);

            if (existing != null && existing.Id != payment.Id)
            {
                throw new TransactionAlreadyRecordedException($"Transaction {reference} has already been recorded against another payment.");
            }
        }

        private static void EnsureTransitionIsAllowed(PaymentStatus current, PaymentStatus next)
        {
            if (current == next)
            {
                throw new InvalidPaymentStatusTransitionException($"Payment is already {current}.");
            }

            var allowed = AllowedTransitions.TryGetValue(current, out var statuses) ? statuses : [];

            if (!allowed.Contains(next))
            {
                throw new InvalidPaymentStatusTransitionException($"A payment cannot move from {current} to {next}.");
            }
        }
    }
}
