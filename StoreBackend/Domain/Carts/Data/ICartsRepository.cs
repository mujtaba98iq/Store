namespace Domain.Carts;

public interface ICartsRepository
{
    Task<Cart> Create(Cart cart);
    Task<Cart> Update(Cart cart);
    Task<Cart?> FindById(Guid id);
    Task<Cart?> FindByUserId(Guid userId);
    Task<List<Cart>> FindByFilters(CartFilters cartFilters);
    Task<int> GetTotalCountByFilters(CartFilters cartFilters);
}
