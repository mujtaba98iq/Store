using Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace Data.Carts;

public class CartItemsRepository(ApplicationDbContext dbContext) : ICartItemsRepository
{
    public async Task<CartItem> Create(CartItem cartItem)
    {
        dbContext.CartItems.Add(cartItem);
        await dbContext.SaveChangesAsync();
        return cartItem;
    }

    public async Task<CartItem?> FindById(Guid id)
    {
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);
        return cartItem;
    }

    public async Task<CartItem?> FindByCartIdAndProductVariantId(Guid cartId, Guid productVariantId)
    {
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cartId
                                      && i.ProductVariantId == productVariantId
                                      && i.DeletedAt == null);
        return cartItem;
    }

    public async Task<List<CartItem>> FindByCartId(Guid cartId)
    {
        return await dbContext.CartItems
            .Where(i => i.CartId == cartId && i.DeletedAt == null)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<CartItem> Update(CartItem cartItem)
    {
        dbContext.CartItems.Update(cartItem);
        await dbContext.SaveChangesAsync();
        return cartItem;
    }

    public async Task<List<CartItem>> UpdateMany(List<CartItem> cartItems)
    {
        dbContext.CartItems.UpdateRange(cartItems);
        await dbContext.SaveChangesAsync();
        return cartItems;
    }
}
