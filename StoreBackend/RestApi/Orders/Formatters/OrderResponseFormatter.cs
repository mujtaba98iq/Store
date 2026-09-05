using Domain.Orders;

namespace RestApi.Orders;

public class OrderResponseFormatter : IOrderResponseFormatter
{
    public OrderListResponse Many(IEnumerable<Order> orders, int totalCount)
    {
        var orderResults = orders.Select(One).ToList();

        return new OrderListResponse
        {
            Data = orderResults,
            TotalCount = totalCount
        };
    }

    public OrderResponse One(Order order)
    {
        var items = order.Items
            .Where(i => i.DeletedAt == null)
            .OrderBy(i => i.CreatedAt)
            .Select(One)
            .ToList();

        // The stored amounts are handed back untouched rather than re-summed from the lines.
        // They are what the customer was billed, and a mismatch between the two is something
        // to notice, not something to paper over here.
        return new OrderResponse
        {
            Id = order.Id.ToString(),
            UserId = order.UserId.ToString(),
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Items = items,
            ShippingAddress = order.ShippingAddress == null ? null : One(order.ShippingAddress),
            ItemCount = items.Count,
            Subtotal = order.Subtotal,
            DiscountAmount = order.DiscountAmount,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CreatedById = order.CreatedById,
            UpdatedById = order.UpdatedById
        };
    }

    private static OrderShippingAddressResponse One(OrderShippingAddress shippingAddress)
    {
        return new OrderShippingAddressResponse
        {
            Id = shippingAddress.Id.ToString(),
            OrderId = shippingAddress.OrderId.ToString(),
            FullName = shippingAddress.FullName,
            PhoneNumber = shippingAddress.PhoneNumber,
            Country = shippingAddress.Country,
            City = shippingAddress.City,
            Area = shippingAddress.Area,
            Street = shippingAddress.Street,
            Building = shippingAddress.Building
        };
    }

    private static OrderItemResponse One(OrderItem orderItem)
    {
        return new OrderItemResponse
        {
            Id = orderItem.Id.ToString(),
            OrderId = orderItem.OrderId.ToString(),
            ProductVariantId = orderItem.ProductVariantId.ToString(),
            ProductName = orderItem.ProductName,
            Sku = orderItem.Sku,
            UnitPrice = orderItem.UnitPrice,
            Quantity = orderItem.Quantity,
            DiscountAmount = orderItem.DiscountAmount,
            TotalAmount = orderItem.TotalAmount,
            CreatedAt = orderItem.CreatedAt
        };
    }
}
