namespace Domain.Inventories;

public interface IInventoriesRepository
{
    Task<Inventory> Create(Inventory inventory);
    Task<Inventory> Update(Inventory inventory);
    Task<Inventory?> FindById(Guid id);
    Task<Inventory?> FindByProductVariantId(Guid productVariantId);
    Task<List<Inventory>> FindByFilters(InventoryFilters inventoryFilters);
    Task<int> GetTotalCountByFilters(InventoryFilters inventoryFilters);
}
