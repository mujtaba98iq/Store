using Domain.Data;
using Domain.Products;
using Domain.Users;

namespace Domain.Wishlists;

/// <summary>
/// A product a customer wants to keep hold of without buying it yet.
///
/// There is no wishlist row above this one. A customer's wishlist is simply the entries
/// carrying their id, so the list exists the moment the first product is added and needs
/// nothing created before it. A parent row would earn its place if a customer could keep
/// several named lists, or share one, because then the list would have properties of its
/// own to hold; with a single list per customer it would carry nothing but the id that is
/// already on every entry.
///
/// The entry points at a product rather than at a variant, as a review does: wanting a
/// shirt is not wanting one size of it. Which variant gets bought is decided at the cart,
/// where a price and a quantity have to be pinned down.
///
/// Nothing on an entry is editable. A customer either wants the product or does not, so
/// the only changes it ever sees are being added and being taken off again.
/// </summary>
public class WishlistItem : IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// Whose list this is. It is the whole of the list's identity, there being no wishlist
    /// row for it to point at instead.
    /// </summary>
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    /// <summary>
    /// Empty unless a read asked for them, which the wishlist's own reads do not: a client
    /// showing a list gets the product ids and fetches the catalogue rows it needs, rather
    /// than having a copy of the catalogue dragged along behind every entry.
    /// </summary>
    public User? User { get; set; }
    public Product? Product { get; set; }
}
