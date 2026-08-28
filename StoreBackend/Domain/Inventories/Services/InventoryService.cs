using Domain.Exeptions;
using Domain.ProductVariants;
using Sheard.Type;

namespace Domain.Inventories
{
    public class InventoryService(IInventoriesRepository inventoriesRepository, IProductVariantsRepository productVariantsRepository) : IInventoryService
    {
        public async Task<Inventory> Create(CreateInventoryParams createInventoryParams)
        {
            _ = await productVariantsRepository.FindById(createInventoryParams.ProductVariantId)
                ?? throw new ResourceNotFoundException("ProductVariant", $"Product variant with ID {createInventoryParams.ProductVariantId} not found");

            await EnsureProductVariantIsNotStockedYet(createInventoryParams.ProductVariantId);

            var quantity = createInventoryParams.Quantity ?? 0;
            var reservedQuantity = createInventoryParams.ReservedQuantity ?? 0;
            EnsureQuantitiesAreConsistent(quantity, reservedQuantity);

            var inventory = new Inventory
            {
                Id = Guid.NewGuid(),
                ProductVariantId = createInventoryParams.ProductVariantId,
                Quantity = quantity,
                ReservedQuantity = reservedQuantity,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createInventoryParams.CreatedById
            };

            return await inventoriesRepository.Create(inventory);
        }

        public async Task<Inventory?> FindById(Guid id)
        {
            return await inventoriesRepository.FindById(id);
        }

        public async Task<Inventory?> FindByProductVariantId(Guid productVariantId)
        {
            return await inventoriesRepository.FindByProductVariantId(productVariantId);
        }

        public async Task<PaginationResult<Inventory>> Search(InventoryFilters inventoryFilters)
        {
            var inventories = await inventoriesRepository.FindByFilters(inventoryFilters);
            var totalCount = await inventoriesRepository.GetTotalCountByFilters(inventoryFilters);

            return new PaginationResult<Inventory>
            {
                TotalCount = totalCount,
                Data = inventories
            };
        }

        public async Task<Inventory> Update(UpdateInventoryParams updateInventoryParams)
        {
            var inventory = await inventoriesRepository.FindById(updateInventoryParams.Id)
                            ?? throw new ResourceNotFoundException("Inventory", $"Inventory with ID {updateInventoryParams.Id} not found");

            var quantity = updateInventoryParams.Quantity ?? inventory.Quantity;
            var reservedQuantity = updateInventoryParams.ReservedQuantity ?? inventory.ReservedQuantity;

            // Checked against the merged values: lowering only Quantity can break the invariant
            // even though the request itself looks harmless.
            EnsureQuantitiesAreConsistent(quantity, reservedQuantity);

            inventory.Quantity = quantity;
            inventory.ReservedQuantity = reservedQuantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            inventory.UpdatedById = updateInventoryParams.UpdatedById;

            return await inventoriesRepository.Update(inventory);
        }

        private async Task EnsureProductVariantIsNotStockedYet(Guid productVariantId)
        {
            var existing = await inventoriesRepository.FindByProductVariantId(productVariantId);
            if (existing != null)
            {
                throw new ResourceAlreadyExistsException(productVariantId.ToString());
            }
        }

        private static void EnsureQuantitiesAreConsistent(int quantity, int reservedQuantity)
        {
            if (quantity < 0)
            {
                throw new InvalidInventoryQuantityException("Quantity cannot be negative.");
            }

            if (reservedQuantity < 0)
            {
                throw new InvalidInventoryQuantityException("ReservedQuantity cannot be negative.");
            }

            if (reservedQuantity > quantity)
            {
                throw new InvalidInventoryQuantityException($"ReservedQuantity ({reservedQuantity}) cannot be greater than Quantity ({quantity}).");
            }
        }
    }
}
