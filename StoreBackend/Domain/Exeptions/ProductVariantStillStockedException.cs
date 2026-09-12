namespace Domain.Exeptions;

/// <summary>
/// Raised when a variant is deleted while a stock row is still open against it.
/// </summary>
/// <remarks>
/// Deleting it anyway would leave that row stocking a variant that no longer exists,
/// and the inventory listing has no way to tell the reader why. So the variant is
/// kept and the caller is pointed at the stock row, which they can delete first.
/// </remarks>
/// <param name="sku">
/// The variant as the caller named it. They sent the ID, but the SKU is what the
/// stock row is labelled by everywhere else, so it is what leads them to it.
/// </param>
public class ProductVariantStillStockedException(string sku)
    : DomainException($"Product variant {sku} still has stock. Delete its stock row first.");
