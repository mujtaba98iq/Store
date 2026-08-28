using Domain.Inventories;

namespace RestApi.Inventories;

public interface IInventoryResponseFormatter
{
    InventoryResponse One(Inventory inventory);
    InventoryListResponse Many(IEnumerable<Inventory> inventories, int totalCount);
}
