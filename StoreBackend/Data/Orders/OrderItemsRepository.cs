using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Data.Orders;

public class OrderItemsRepository(ApplicationDbContext dbContext) : IOrderItemsRepository
{
    public async Task<OrderItem?> FindById(Guid id)
    {
        var orderItem = await dbContext.OrderItems
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);
        return orderItem;
    }

    public async Task<List<OrderItem>> FindByOrderId(Guid orderId)
    {
        return await dbContext.OrderItems
            .AsNoTracking()
            .Where(i => i.OrderId == orderId && i.DeletedAt == null)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ContainsProduct(Guid orderId, Guid productId)
    {
        // Asked of the database rather than by walking the lines in memory: the answer is a
        // yes or a no, and the variant each line points at would otherwise be a query of its
        // own.
        //
        // The variant is read whatever state the catalogue has left it in. A product retired
        // or a variant taken down since the order went out was still bought, and the customer
        // who bought it keeps their say.
        return await dbContext.OrderItems
            .AsNoTracking()
            .AnyAsync(i => i.OrderId == orderId
                           && i.DeletedAt == null
                           && i.ProductVariant != null
                           && i.ProductVariant.ProductId == productId);
    }

    public async Task<List<OrderItem>> CreateMany(List<OrderItem> orderItems)
    {
        dbContext.OrderItems.AddRange(orderItems);
        await dbContext.SaveChangesAsync();
        return orderItems;
    }
}
