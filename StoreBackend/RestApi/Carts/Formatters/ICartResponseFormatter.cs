using Domain.Carts;

namespace RestApi.Carts;

public interface ICartResponseFormatter
{
    CartResponse One(Cart cart);
    CartListResponse Many(IEnumerable<Cart> carts, int totalCount);
}
