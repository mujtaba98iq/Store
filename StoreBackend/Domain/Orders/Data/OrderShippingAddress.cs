using Domain.Data;

namespace Domain.Orders;

/// <summary>
/// Where an order was sent, as it read at checkout. A copy, never a pointer at whatever
/// address the customer keeps on file today: they move, they fix a typo, they switch to a
/// work address, and none of that may change where an order that already shipped went.
///
/// The contact details are copied for the same reason. The name and number on a delivery
/// note are the ones the courier was given, not the ones on the account this morning.
/// </summary>
public class OrderShippingAddress : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>
    /// Who the parcel is addressed to, which is not always the account holder.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// The number the courier calls on arrival.
    /// </summary>
    public required string PhoneNumber { get; set; }

    public required string Country { get; set; }
    public required string City { get; set; }

    /// <summary>
    /// District or neighbourhood. Optional: not every address is given with one.
    /// </summary>
    public string? Area { get; set; }

    public required string Street { get; set; }

    /// <summary>
    /// House or building number.
    /// </summary>
    public required string Building { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Order? Order { get; set; }
}
