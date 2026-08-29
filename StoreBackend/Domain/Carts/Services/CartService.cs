using Domain.Exeptions;
using Domain.ProductVariants;
using Domain.Products;
using Domain.Users;
using Sheard.Type;

namespace Domain.Carts
{
    public class CartService(
        ICartsRepository cartsRepository,
        ICartItemsRepository cartItemsRepository,
        IProductVariantsRepository productVariantsRepository,
        IProductsRepository productsRepository,
        IUsersRepository usersRepository) : ICartService
    {
        public async Task<Cart> GetOrCreateByUserId(GetOrCreateCartParams getOrCreateCartParams)
        {
            var cart = await cartsRepository.FindByUserId(getOrCreateCartParams.UserId);
            if (cart != null)
            {
                return cart;
            }

            _ = await usersRepository.FindById(getOrCreateCartParams.UserId)
                ?? throw new ResourceNotFoundException("User", $"User with ID {getOrCreateCartParams.UserId} not found");

            return await cartsRepository.Create(new Cart
            {
                Id = Guid.NewGuid(),
                UserId = getOrCreateCartParams.UserId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = getOrCreateCartParams.CreatedById
            });
        }

        public async Task<Cart?> FindById(Guid id)
        {
            return await cartsRepository.FindById(id);
        }

        public async Task<Cart?> FindByUserId(Guid userId)
        {
            return await cartsRepository.FindByUserId(userId);
        }

        public async Task<PaginationResult<Cart>> Search(CartFilters cartFilters)
        {
            var carts = await cartsRepository.FindByFilters(cartFilters);
            var totalCount = await cartsRepository.GetTotalCountByFilters(cartFilters);

            return new PaginationResult<Cart>
            {
                TotalCount = totalCount,
                Data = carts
            };
        }

        public async Task<Cart> AddItem(AddCartItemParams addCartItemParams)
        {
            EnsureQuantityIsPositive(addCartItemParams.Quantity);

            var cart = await GetOrCreateByUserId(new GetOrCreateCartParams
            {
                UserId = addCartItemParams.UserId,
                CreatedById = addCartItemParams.CreatedById
            });

            var unitPrice = await ResolveUnitPrice(addCartItemParams.ProductVariantId);
            var existingItem = await cartItemsRepository.FindByCartIdAndProductVariantId(cart.Id, addCartItemParams.ProductVariantId);

            if (existingItem == null)
            {
                await cartItemsRepository.Create(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductVariantId = addCartItemParams.ProductVariantId,
                    Quantity = addCartItemParams.Quantity,
                    UnitPrice = unitPrice,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = addCartItemParams.CreatedById
                });
            }
            else
            {
                // Adding the same variant twice tops up the line instead of listing it again,
                // and the whole line is repriced: one line can only carry one unit price.
                existingItem.Quantity += addCartItemParams.Quantity;
                existingItem.UnitPrice = unitPrice;
                existingItem.UpdatedAt = DateTime.UtcNow;
                existingItem.UpdatedById = addCartItemParams.CreatedById;

                await cartItemsRepository.Update(existingItem);
            }

            return await TouchAndReload(cart, addCartItemParams.CreatedById);
        }

        public async Task<Cart> UpdateItem(UpdateCartItemParams updateCartItemParams)
        {
            // Zero is rejected rather than treated as a removal, so a client cannot drop a
            // line by accident when it meant to send a real quantity.
            EnsureQuantityIsPositive(updateCartItemParams.Quantity);

            var cart = await FindCartOfUser(updateCartItemParams.UserId);
            var cartItem = await FindItemOfCart(cart.Id, updateCartItemParams.CartItemId);

            cartItem.Quantity = updateCartItemParams.Quantity;
            cartItem.UnitPrice = await ResolveUnitPrice(cartItem.ProductVariantId);
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.UpdatedById = updateCartItemParams.UpdatedById;

            await cartItemsRepository.Update(cartItem);

            return await TouchAndReload(cart, updateCartItemParams.UpdatedById);
        }

        public async Task<Cart> RemoveItem(RemoveCartItemParams removeCartItemParams)
        {
            var cart = await FindCartOfUser(removeCartItemParams.UserId);
            var cartItem = await FindItemOfCart(cart.Id, removeCartItemParams.CartItemId);

            cartItem.DeletedAt = DateTime.UtcNow;
            cartItem.DeletedById = removeCartItemParams.DeletedById;

            await cartItemsRepository.Update(cartItem);

            return await TouchAndReload(cart, removeCartItemParams.DeletedById);
        }

        public async Task<Cart> Clear(ClearCartParams clearCartParams)
        {
            var cart = await FindCartOfUser(clearCartParams.UserId);
            var cartItems = await cartItemsRepository.FindByCartId(cart.Id);

            if (cartItems.Count == 0)
            {
                return cart;
            }

            var deletedAt = DateTime.UtcNow;
            foreach (var cartItem in cartItems)
            {
                cartItem.DeletedAt = deletedAt;
                cartItem.DeletedById = clearCartParams.DeletedById;
            }

            await cartItemsRepository.UpdateMany(cartItems);

            return await TouchAndReload(cart, clearCartParams.DeletedById);
        }

        /// <summary>
        /// A cart is only ever reached through the id of the customer who owns it, so a line
        /// belonging to somebody else can never be addressed.
        /// </summary>
        private async Task<Cart> FindCartOfUser(Guid userId)
        {
            return await cartsRepository.FindByUserId(userId)
                   ?? throw new ResourceNotFoundException("Cart", $"Cart for user with ID {userId} not found");
        }

        private async Task<CartItem> FindItemOfCart(Guid cartId, Guid cartItemId)
        {
            var cartItem = await cartItemsRepository.FindById(cartItemId);

            return cartItem == null || cartItem.CartId != cartId
                ? throw new ResourceNotFoundException("CartItem", $"Cart item with ID {cartItemId} not found")
                : cartItem;
        }

        /// <summary>
        /// A cart is a container: any change to its lines is a change to the cart itself, so
        /// its audit stamp moves with them. Reloading returns the lines as they now stand.
        /// </summary>
        private async Task<Cart> TouchAndReload(Cart cart, string updatedById)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedById = updatedById;

            await cartsRepository.Update(cart);

            return await cartsRepository.FindById(cart.Id) ?? cart;
        }

        /// <summary>
        /// A variant may carry its own price or fall back to the price of its product.
        /// </summary>
        private async Task<decimal> ResolveUnitPrice(Guid productVariantId)
        {
            var productVariant = await productVariantsRepository.FindById(productVariantId)
                                 ?? throw new ResourceNotFoundException("ProductVariant", $"Product variant with ID {productVariantId} not found");

            if (!productVariant.IsActive)
            {
                throw new ProductVariantNotPurchasableException($"Product variant {productVariantId} is not active.");
            }

            if (productVariant.Price.HasValue)
            {
                return productVariant.Price.Value;
            }

            var product = await productsRepository.FindById(productVariant.ProductId)
                          ?? throw new ResourceNotFoundException("Product", $"Product with ID {productVariant.ProductId} not found");

            return product.Price
                   ?? throw new ProductVariantNotPurchasableException($"Product variant {productVariantId} has no price.");
        }

        private static void EnsureQuantityIsPositive(int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidCartQuantityException("Quantity must be greater than zero.");
            }
        }
    }
}
