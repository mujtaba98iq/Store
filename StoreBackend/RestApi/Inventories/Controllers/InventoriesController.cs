using Domain.Inventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestApi.Extensions;
using UseValidator;

namespace RestApi.Inventories.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController(IInventoryService inventoryService, IInventoryResponseFormatter responseFormatter) : ControllerBase
    {
        [Authorize(Roles = "User,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InventoryFilters inventoryFilters)
        {
            var inventories = await inventoryService.Search(inventoryFilters);
            return Ok(responseFormatter.Many(inventories.Data, inventories.TotalCount));
        }

        [HttpPost]
        [ProducesResponseType(typeof(InventoryResponse), 201)]
        [UseBodyValidator(Validator = typeof(CreateInventoryRequestValidator))]
        public async Task<IActionResult> Create([FromBody] CreateInventoryRequest createInventoryRequest)
        {
            CreateInventoryParams createInventoryParams = new()
            {
                ProductVariantId = createInventoryRequest.ProductVariantId,
                Quantity = createInventoryRequest.Quantity,
                ReservedQuantity = createInventoryRequest.ReservedQuantity,
                CreatedById = this.GetUserId()
            };

            var inventory = await inventoryService.Create(createInventoryParams);
            var result = responseFormatter.One(inventory);

            return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, result);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InventoryResponse), 200)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var inventory = await inventoryService.FindById(id);
            return inventory is null
                ? NotFound()
                : Ok(responseFormatter.One(inventory));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("product-variant/{productVariantId}")]
        [ProducesResponseType(typeof(InventoryResponse), 200)]
        public async Task<IActionResult> GetByProductVariantId(Guid productVariantId)
        {
            var inventory = await inventoryService.FindByProductVariantId(productVariantId);
            return inventory is null
                ? NotFound()
                : Ok(responseFormatter.One(inventory));
        }

        [HttpPatch("{inventoryId}")]
        [ProducesResponseType(typeof(InventoryResponse), 200)]
        [ProducesResponseType(typeof(object), 404)]
        [UseBodyValidator(Validator = typeof(UpdateInventoryRequestValidator))]
        public async Task<IActionResult> Update(Guid inventoryId, [FromBody] UpdateInventoryRequest request)
        {
            var updatedInventory = await inventoryService.Update(new UpdateInventoryParams
            {
                Id = inventoryId,
                Quantity = request.Quantity,
                ReservedQuantity = request.ReservedQuantity,
                UpdatedById = this.GetUserId()
            });

            return Ok(responseFormatter.One(updatedInventory));
        }
    }
}
