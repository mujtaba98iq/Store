namespace Domain.Wishlists;

/// <summary>
/// When the product was added is the only thing there is to sort by: an entry holds nothing
/// else of its own, and anything a customer would rather sort by — price, name, whether it
/// is back in stock — belongs to the product and is ordered where the catalogue is read.
/// </summary>
public enum WishlistItemOrderBy
{
    CreatedAt = 1,
}
