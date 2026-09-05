using Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Sheard.Type;

namespace Data.Payments;

public class PaymentsRepository(ApplicationDbContext dbContext) : IPaymentsRepository
{
    public async Task<Payment> Create(Payment payment)
    {
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();
        return payment;
    }

    public async Task<List<Payment>> FindByFilters(PaymentFilters paymentFilters)
    {
        var query = WithOrder(dbContext.Payments.AsNoTracking())
            .Where(p => p.DeletedAt == null)
            .AsQueryable();

        query = ApplyFilters(query, paymentFilters);
        query = ApplyOrdering(query, paymentFilters);
        query = ApplyPagination(query, paymentFilters);

        return await query.ToListAsync();
    }

    private static IQueryable<Payment> ApplyPagination(IQueryable<Payment> query, PaymentFilters paymentFilters)
    {
        var page = paymentFilters.Page <= 0 ? 1 : paymentFilters.Page;
        var pageSize = paymentFilters.PageSize <= 0 ? 10 : paymentFilters.PageSize;

        var skip = (page - 1) * pageSize;

        return query.Skip(skip).Take(pageSize);
    }

    private static IQueryable<Payment> ApplyOrdering(IQueryable<Payment> query, PaymentFilters paymentFilters)
    {
        var paymentOrderBy = paymentFilters.OrderBy ?? PaymentOrderBy.CreatedAt;
        var orderDirection = paymentFilters.OrderByDirection ?? OrderDirection.Desc;

        return paymentOrderBy switch
        {
            PaymentOrderBy.CreatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            PaymentOrderBy.UpdatedAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.UpdatedAt)
                : query.OrderByDescending(p => p.UpdatedAt),
            PaymentOrderBy.Amount => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.Amount)
                : query.OrderByDescending(p => p.Amount),
            PaymentOrderBy.PaidAt => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.PaidAt)
                : query.OrderByDescending(p => p.PaidAt),
            // Sorts by the lifecycle order the enum is numbered in, so everything still
            // outstanding groups ahead of what has been settled.
            PaymentOrderBy.PaymentStatus => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.PaymentStatus)
                : query.OrderByDescending(p => p.PaymentStatus),
            _ => orderDirection == OrderDirection.Asc
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };
    }

    private static IQueryable<Payment> ApplyFilters(IQueryable<Payment> query, PaymentFilters paymentFilters)
    {
        if (paymentFilters.PaymentId != null)
        {
            query = query.Where(p => p.Id == paymentFilters.PaymentId);
        }

        if (paymentFilters.OrderId != null)
        {
            query = query.Where(p => p.OrderId == paymentFilters.OrderId);
        }

        // Reached through the order, a payment having no customer of its own. It is what
        // lets a customer be shown their own payments without being handed an order id
        // first.
        if (paymentFilters.UserId != null)
        {
            query = query.Where(p => p.Order != null && p.Order.UserId == paymentFilters.UserId);
        }

        if (paymentFilters.PaymentMethod.HasValue)
        {
            query = query.Where(p => p.PaymentMethod == paymentFilters.PaymentMethod.Value);
        }

        if (paymentFilters.PaymentStatus.HasValue)
        {
            query = query.Where(p => p.PaymentStatus == paymentFilters.PaymentStatus.Value);
        }

        if (paymentFilters.CreatedFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= paymentFilters.CreatedFrom.Value);
        }

        if (paymentFilters.CreatedTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= paymentFilters.CreatedTo.Value);
        }

        if (paymentFilters.MinAmount.HasValue)
        {
            query = query.Where(p => p.Amount >= paymentFilters.MinAmount.Value);
        }

        if (paymentFilters.MaxAmount.HasValue)
        {
            query = query.Where(p => p.Amount <= paymentFilters.MaxAmount.Value);
        }

        return query;
    }

    public async Task<Payment?> FindById(Guid id)
    {
        var payment = await WithOrder(dbContext.Payments.AsNoTracking())
            .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);
        return payment;
    }

    public async Task<Payment?> FindByTransactionId(string transactionId)
    {
        var payment = await WithOrder(dbContext.Payments.AsNoTracking())
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId && p.DeletedAt == null);
        return payment;
    }

    public async Task<List<Payment>> FindByOrderId(Guid orderId)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == orderId && p.DeletedAt == null)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment> Update(Payment payment)
    {
        // Written straight to the row rather than through the change tracker, as orders are:
        // the payment carries the order it was loaded with, and saving it as a graph would
        // write that back too. Only the status, the reference and the audit columns move —
        // the amount and the method are what the attempt was made for and do not change.
        await dbContext.Payments
            .Where(p => p.Id == payment.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.PaymentStatus, payment.PaymentStatus)
                .SetProperty(p => p.TransactionId, payment.TransactionId)
                .SetProperty(p => p.PaidAt, payment.PaidAt)
                .SetProperty(p => p.UpdatedAt, payment.UpdatedAt)
                .SetProperty(p => p.UpdatedById, payment.UpdatedById)
                .SetProperty(p => p.DeletedAt, payment.DeletedAt)
                .SetProperty(p => p.DeletedById, payment.DeletedById));

        return payment;
    }

    public async Task<int> GetTotalCountByFilters(PaymentFilters paymentFilters)
    {
        var query = dbContext.Payments.AsNoTracking()
            .Where(p => p.DeletedAt == null)
            .AsQueryable();
        query = ApplyFilters(query, paymentFilters);
        return await query.CountAsync();
    }

    private static IQueryable<Payment> WithOrder(IQueryable<Payment> query)
    {
        // The order comes back with the payment because every reader needs it: the customer
        // it belongs to is the only thing that says who may look at the payment at all.
        return query.Include(p => p.Order);
    }
}
