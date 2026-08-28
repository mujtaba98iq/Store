using Domain.Inventories;

namespace RestApi.Inventories;

public class InventoryResponseFormatter : IInventoryResponseFormatter
{
    public InventoryListResponse Many(IEnumerable<Inventory> inventories, int totalCount)
    {
        var inventoryResults = inventories.Select(One).ToList();

        return new InventoryListResponse
        {
            Data = inventoryResults,
            TotalCount = totalCount
        };
    }

    public InventoryResponse One(Inventory inventory)
    {
        return new InventoryResponse
        {
            Id = inventory.Id.ToString(),
            ProductVariantId = inventory.ProductVariantId.ToString(),
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = inventory.AvailableQuantity,
            CreatedAt = inventory.CreatedAt,
            UpdatedAt = inventory.UpdatedAt,
            CreatedById = inventory.CreatedById,
            UpdatedById = inventory.UpdatedById
        };
    }
}
