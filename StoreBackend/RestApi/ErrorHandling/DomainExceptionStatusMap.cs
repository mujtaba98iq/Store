using Domain;
using Domain.Exeptions;

namespace RestApi.ErrorHandling;

/// <summary>
/// Which HTTP status each broken rule answers with.
/// </summary>
/// <remarks>
/// The mapping lives here rather than on the exceptions themselves: the Domain has
/// no idea it is being reached over HTTP, and nothing in this table changes what a
/// rule means - only how this one transport reports it.
/// </remarks>
internal static class DomainExceptionStatusMap
{
    /// <summary>
    /// A rule broken because the request asked for something the current state of
    /// the shop will not allow - a name in use, stock that is not there, a status
    /// the order cannot reach from where it stands. The caller can act on all of
    /// these, but not by rewriting the request.
    /// </summary>
    private static readonly HashSet<Type> Conflicts =
    [
        typeof(ResourceAlreadyExistsException),
        typeof(CouponCodeAlreadyExistsException),
        typeof(CouponNotActiveException),
        typeof(CouponUsageLimitReachedException),
        typeof(InsufficientStockException),
        typeof(InvalidOrderStatusTransitionException),
        typeof(InvalidPaymentStatusTransitionException),
        typeof(InvalidShipmentStatusTransitionException),
        typeof(OrderAlreadyPaidException),
        typeof(OrderNotPayableException),
        typeof(ProductAlreadyReviewedException),
        typeof(ProductVariantNotPurchasableException),
        typeof(ProductVariantStillStockedException),
        typeof(ShipmentAlreadyExistsException),
        typeof(TransactionAlreadyRecordedException),
    ];

    /// <summary>
    /// Everything else is the request's own fault - a quantity that cannot be, a
    /// rating off the scale, a checkout with an empty cart. A rule added later and
    /// never listed above lands here, which keeps a new business failure reported
    /// as the caller's to fix rather than as a fault in the server.
    /// </summary>
    private const int Default = StatusCodes.Status400BadRequest;

    public static int StatusFor(DomainException exception)
    {
        if (exception is ResourceNotFoundException)
        {
            return StatusCodes.Status404NotFound;
        }

        return Conflicts.Contains(exception.GetType())
            ? StatusCodes.Status409Conflict
            : Default;
    }
}
