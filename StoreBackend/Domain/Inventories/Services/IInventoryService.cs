using Sheard.Type;

namespace Domain.Inventories;

public interface IInventoryService
{
    Task<Inventory> Create(CreateInventoryParams createInventoryParams);
    Task<Inventory?> FindById(Guid id);
    Task<Inventory?> FindByProductVariantId(Guid productVariantId);
    Task<Inventory> Update(UpdateInventoryParams updateInventoryParams);
    Task<PaginationResult<Inventory>> Search(InventoryFilters inventoryFilters);

    /// <summary>Soft-deletes a stock row. False when it does not exist, or was already deleted.</summary>
    Task<bool> Delete(DeleteInventoryParams deleteInventoryParams);
}
