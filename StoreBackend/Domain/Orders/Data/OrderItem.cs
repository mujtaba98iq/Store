using Domain.Data;
using Domain.ProductVariants;

namespace Domain.Orders;

/// <summary>
/// One product variant on an order, at the quantity and price it was bought for.
///
/// The catalogue fields here are copies taken at checkout, never lookups. A product can be
/// renamed, repriced, re-SKUd or retired long after the fact and none of it may change what
/// this line says the customer bought: an iPhone ordered at 800 stays an 800 line after the
/// catalogue moves to 900.
/// </summary>
public class OrderItem : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>
    /// Kept so a line can still be traced back to the catalogue. It is a reference, not a
    /// source: everything needed to read the line is copied below.
    /// </summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Name of the product as it read at checkout.
    /// </summary>
    public required string ProductName { get; set; }

    /// <summary>
    /// SKU of the variant as it read at checkout.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// What one unit cost at checkout.
    /// </summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Money taken off this line in particular, as opposed to the order-wide discount that
    /// sits on <see cref="Order.DiscountAmount"/>.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Quantity * UnitPrice - DiscountAmount, as charged. Stored rather than derived, for
    /// the same reason as the rest of the line: this is a record of what was billed, not a
    /// sum to be recomputed later.
    /// </summary>
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Order? Order { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
