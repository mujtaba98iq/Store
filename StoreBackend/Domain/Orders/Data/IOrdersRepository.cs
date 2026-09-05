namespace Domain.Orders;

public interface IOrdersRepository
{
    Task<Order> Create(Order order);
    Task<Order> Update(Order order);
    Task<Order?> FindById(Guid id);
    Task<Order?> FindByOrderNumber(string orderNumber);
    Task<List<Order>> FindByFilters(OrderFilters orderFilters);
    Task<int> GetTotalCountByFilters(OrderFilters orderFilters);
}
