using Domain.Data;
using Domain.ProductVariants;

namespace Domain.Carts;

/// <summary>
/// One product variant in a cart, together with the quantity wanted and the price
/// it is currently offered at.
/// </summary>
public class CartItem : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }

    /// <summary>
    /// Price of a single unit, copied from the variant when the line is written. The cart
    /// carries its own copy so a later price change cannot silently rewrite what the
    /// customer is looking at mid-session.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Quantity * UnitPrice. Derived, so it must never become a column.
    /// </summary>
    public decimal Subtotal => Quantity * UnitPrice;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public Cart? Cart { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
