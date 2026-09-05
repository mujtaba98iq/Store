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

    public async Task<List<OrderItem>> CreateMany(List<OrderItem> orderItems)
    {
        dbContext.OrderItems.AddRange(orderItems);
        await dbContext.SaveChangesAsync();
        return orderItems;
    }
}
