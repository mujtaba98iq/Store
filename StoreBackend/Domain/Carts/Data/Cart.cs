using Domain.Data;
using Domain.Users;

namespace Domain.Carts;

/// <summary>
/// The products a customer has picked but not ordered yet. A customer keeps a single
/// cart that is emptied on checkout rather than a new one per shopping session.
/// </summary>
public class Cart : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// Sum of every line still in the cart. Derived, so it must never become a column.
    /// </summary>
    public decimal TotalAmount => Items.Where(i => i.DeletedAt == null).Sum(i => i.Subtotal);

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
