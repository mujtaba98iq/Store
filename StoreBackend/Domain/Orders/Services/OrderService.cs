using Domain.Carts;
using Domain.Coupons;
using Domain.Exeptions;
using Domain.Inventories;
using Domain.Payments;
using Domain.ProductVariants;
using Domain.Products;
using Domain.Shipments;
using Sheard.Type;

namespace Domain.Orders
{
    public class OrderService(
        IOrdersRepository ordersRepository,
        IOrderItemsRepository orderItemsRepository,
        ICartService cartService,
        IInventoryService inventoryService,
        IProductVariantsRepository productVariantsRepository,
        IProductsRepository productsRepository,
        IPaymentService paymentService,
        IShipmentService shipmentService,
        ICouponService couponService) : IOrderService
    {
        /// <summary>
        /// A cart line with everything the order needs already copied off the catalogue.
        /// Gathered for every line before a single one is written, so the catalogue is read
        /// exactly once per line and never again.
        /// </summary>
        private sealed record CheckoutLine(CartItem CartItem, string ProductName, string Sku)
        {
            /// <summary>
            /// Nothing comes off an individual line. Coupons discount the order as a whole
            /// and sit on <see cref="Order.DiscountAmount"/>; nothing yet decides what a
            /// single line is worth on its own. The column exists so that when something
            /// does, it has somewhere to write.
            /// </summary>
            public decimal DiscountAmount => decimal.Zero;

            public decimal TotalAmount => (CartItem.Quantity * CartItem.UnitPrice) - DiscountAmount;
        }

        /// <summary>
        /// Where an order may go next. Anything missing from a status's list is refused,
        /// which is what stops a shipped order being cancelled and a cancelled one coming
        /// back to life. Delivered and Cancelled are ends of the line.
        /// </summary>
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
            [OrderStatus.Confirmed] = [OrderStatus.Processing, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Delivered],
            [OrderStatus.Delivered] = [],
            [OrderStatus.Cancelled] = [],
        };

        private const int OrderNumberAttempts = 5;

        public async Task<Order> Checkout(CheckoutParams checkoutParams)
        {
            var cart = await cartService.FindByUserId(checkoutParams.UserId)
                       ?? throw new ResourceNotFoundException("Cart", $"Cart for user with ID {checkoutParams.UserId} not found");

            var cartItems = cart.Items
                .Where(i => i.DeletedAt == null)
                .OrderBy(i => i.CreatedAt)
                .ToList();

            if (cartItems.Count == 0)
            {
                throw new EmptyCartCheckoutException("Cannot check out an empty cart.");
            }

            // Every line is checked, and its catalogue copy taken, before any of them is
            // written. A checkout that fails on the last line must not leave stock reserved
            // against the ones ahead of it.
            var lines = new List<CheckoutLine>();
            foreach (var cartItem in cartItems)
            {
                var productVariant = await FindPurchasableVariant(cartItem.ProductVariantId);
                var product = await FindProduct(productVariant.ProductId);

                _ = await FindInventoryWithStock(cartItem.ProductVariantId, cartItem.Quantity);

                lines.Add(new CheckoutLine(cartItem, product.Name, productVariant.Sku));
            }

            var subtotal = lines.Sum(line => line.TotalAmount);

            // Redeemed after every line has been priced and found in stock, so a checkout
            // that was going to fail anyway does not spend one of a limited campaign's
            // redemptions on its way out.
            //
            // It is still spent before the order is written, and that is the deliberate
            // choice: the alternative is to book it afterwards, which would let two customers
            // both pass the limit check and both get through. A redemption stranded by a
            // failure further down is the same kind of loose end as stock left reserved by
            // one, and is put right the same way, by hand.
            var couponApplication = await RedeemCoupon(checkoutParams, subtotal);
            var discountAmount = couponApplication?.DiscountAmount ?? decimal.Zero;

            EnsureAmountsAreConsistent(subtotal, discountAmount, checkoutParams.ShippingAmount);

            var placedAt = DateTime.UtcNow;

            var orderId = Guid.NewGuid();

            // The address is written as part of the order itself rather than through a call
            // of its own: a delivery that cannot say where it went is not worth keeping, so
            // the two rows land together or neither does.
            var order = await ordersRepository.Create(new Order
            {
                Id = orderId,
                UserId = checkoutParams.UserId,
                OrderNumber = await GenerateOrderNumber(),
                Status = OrderStatus.Pending,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,

                // The code is copied as well as pointed at, so the order can still say what
                // was quoted after the campaign is edited or withdrawn.
                CouponId = couponApplication?.Coupon.Id,
                CouponCode = couponApplication?.Coupon.Code,
                ShippingAmount = checkoutParams.ShippingAmount,
                TotalAmount = subtotal - discountAmount + checkoutParams.ShippingAmount,
                CreatedAt = placedAt,
                CreatedById = checkoutParams.CreatedById,
                ShippingAddress = BuildShippingAddress(orderId, checkoutParams, placedAt)
            });

            // The unit price comes off the cart line, not the variant: the customer pays what
            // they were shown, even if the catalogue moved while they were deciding. Name and
            // SKU are copied for the same reason, so the line stays readable years later.
            await orderItemsRepository.CreateMany(lines
                .Select(line => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = line.CartItem.ProductVariantId,
                    ProductName = line.ProductName,
                    Sku = line.Sku,
                    UnitPrice = line.CartItem.UnitPrice,
                    Quantity = line.CartItem.Quantity,
                    DiscountAmount = line.DiscountAmount,
                    TotalAmount = line.TotalAmount,
                    CreatedAt = placedAt,
                    CreatedById = checkoutParams.CreatedById
                })
                .ToList());

            foreach (var line in lines)
            {
                await ReserveStock(line.CartItem.ProductVariantId, line.CartItem.Quantity, checkoutParams.CreatedById);
            }

            // Both are opened once the goods are actually held. A payment recorded against
            // stock that turned out not to be there would have to be unpicked, and a parcel
            // would be queued for a warehouse with nothing to pick.
            //
            // The payment starts pending whatever the method: a card is settled when the
            // provider answers, cash when the courier hands the parcel over. The shipment
            // starts pending too, which is a warehouse queue rather than a claim that
            // anything has moved.
            await paymentService.Record(new RecordPaymentParams
            {
                OrderId = order.Id,
                PaymentMethod = checkoutParams.PaymentMethod,
                CreatedById = checkoutParams.CreatedById
            });

            await shipmentService.Create(new CreateShipmentParams
            {
                OrderId = order.Id,
                CreatedById = checkoutParams.CreatedById
            });

            // Emptied last: until the order and its lines are on disk, the cart is the only
            // record of what the customer asked for.
            await cartService.Clear(new ClearCartParams
            {
                UserId = checkoutParams.UserId,
                DeletedById = checkoutParams.CreatedById
            });

            return await ordersRepository.FindById(order.Id) ?? order;
        }

        /// <summary>
        /// Spends the quoted coupon, or returns null where none was quoted.
        ///
        /// A blank code counts as none rather than as a code that does not exist: a client
        /// sending an empty field is a customer who did not fill the box in, and refusing
        /// their checkout over it would be nonsense. Anything else is looked up, and a code
        /// that cannot be used stops the checkout — the customer chose to buy at a discount,
        /// and quietly billing them full price instead is not the shop's decision to make.
        /// </summary>
        private async Task<CouponApplication?> RedeemCoupon(CheckoutParams checkoutParams, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(checkoutParams.CouponCode))
            {
                return null;
            }

            return await couponService.Redeem(new RedeemCouponParams
            {
                Code = checkoutParams.CouponCode,
                Subtotal = subtotal,
                UpdatedById = checkoutParams.CreatedById
            });
        }

        /// <summary>
        /// Copies the address off the checkout instead of pointing the order at one. The
        /// customer may move, correct a typo or send the next order somewhere else entirely,
        /// and an order that already went out must keep saying where it went.
        /// </summary>
        private static OrderShippingAddress BuildShippingAddress(Guid orderId, CheckoutParams checkoutParams, DateTime placedAt)
        {
            var shippingAddress = checkoutParams.ShippingAddress;

            return new OrderShippingAddress
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                FullName = shippingAddress.FullName,
                PhoneNumber = shippingAddress.PhoneNumber,
                Country = shippingAddress.Country,
                City = shippingAddress.City,
                Area = shippingAddress.Area,
                Street = shippingAddress.Street,
                Building = shippingAddress.Building,
                CreatedAt = placedAt,
                CreatedById = checkoutParams.CreatedById
            };
        }

        public async Task<Order?> FindById(Guid id)
        {
            return await ordersRepository.FindById(id);
        }

        public async Task<PaginationResult<Order>> Search(OrderFilters orderFilters)
        {
            var orders = await ordersRepository.FindByFilters(orderFilters);
            var totalCount = await ordersRepository.GetTotalCountByFilters(orderFilters);

            return new PaginationResult<Order>
            {
                TotalCount = totalCount,
                Data = orders
            };
        }

        public async Task<Order> UpdateStatus(UpdateOrderStatusParams updateOrderStatusParams)
        {
            var order = await FindOrThrow(updateOrderStatusParams.OrderId);

            return await Transition(order, updateOrderStatusParams.Status, updateOrderStatusParams.UpdatedById);
        }

        public async Task<Order> Cancel(CancelOrderParams cancelOrderParams)
        {
            var order = await FindOrThrow(cancelOrderParams.OrderId);

            // Someone else's order is reported as missing rather than refused: whether it
            // exists is not something a stranger should be able to learn.
            if (order.UserId != cancelOrderParams.UserId)
            {
                throw new ResourceNotFoundException("Order", $"Order with ID {cancelOrderParams.OrderId} not found");
            }

            return await Transition(order, OrderStatus.Cancelled, cancelOrderParams.UpdatedById);
        }

        private async Task<Order> Transition(Order order, OrderStatus status, string updatedById)
        {
            EnsureTransitionIsAllowed(order.Status, status);

            // Stock moves before the status does. If an inventory write fails the order stays
            // where it was and the call can be repeated; a status that ran ahead of the stock
            // it stands for cannot be put right by repeating anything.
            switch (status)
            {
                case OrderStatus.Shipped:
                    await SettleReservedStock(order, updatedById);
                    break;
                case OrderStatus.Cancelled:
                    await ReleaseReservedStock(order, updatedById);
                    break;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedById = updatedById;

            await ordersRepository.Update(order);

            await SyncPaymentAndShipment(order, status, updatedById);

            return await ordersRepository.FindById(order.Id) ?? order;
        }

        /// <summary>
        /// Brings the money and the parcel into line with an order that has just moved. Both
        /// follow rather than lead, and both run after the status is written: the order is
        /// the record of what happened, and a follower that failed first would block it.
        /// </summary>
        private async Task SyncPaymentAndShipment(Order order, OrderStatus status, string updatedById)
        {
            switch (status)
            {
                case OrderStatus.Shipped:
                    await shipmentService.SyncWithOrderStatus(order.Id, ShipmentStatus.Shipped, updatedById);
                    break;

                case OrderStatus.Delivered:
                    await shipmentService.SyncWithOrderStatus(order.Id, ShipmentStatus.Delivered, updatedById);

                    // Delivery is the moment cash actually changes hands, so this is where a
                    // cash order is settled. A card left pending at the door never came
                    // through, and is deliberately left alone for somebody to look at.
                    await paymentService.SettleCashOnDelivery(order.Id, updatedById);
                    break;

                case OrderStatus.Cancelled:
                    // Only what was never collected. Money already taken goes back through an
                    // explicit refund, which is a decision for staff rather than a side
                    // effect of cancelling.
                    await paymentService.VoidOutstanding(order.Id, updatedById);
                    break;
            }
        }

        /// <summary>
        /// Shipping turns a reservation into a real deduction: the units have left, so they
        /// come off the shelf count and off the reserved count together.
        /// </summary>
        private async Task SettleReservedStock(Order order, string updatedById)
        {
            foreach (var orderItem in await orderItemsRepository.FindByOrderId(order.Id))
            {
                var inventory = await inventoryService.FindByProductVariantId(orderItem.ProductVariantId);
                if (inventory == null)
                {
                    continue;
                }

                // Clamped because the counters can be edited by hand through the inventory
                // endpoints. A number that has drifted should not trap an order mid-lifecycle.
                var quantity = Math.Max(0, inventory.Quantity - orderItem.Quantity);
                var reservedQuantity = Math.Min(quantity, Math.Max(0, inventory.ReservedQuantity - orderItem.Quantity));

                await inventoryService.Update(new UpdateInventoryParams
                {
                    Id = inventory.Id,
                    Quantity = quantity,
                    ReservedQuantity = reservedQuantity,
                    UpdatedById = updatedById
                });
            }
        }

        /// <summary>
        /// Cancelling puts the units back within reach of other customers. The shelf count is
        /// untouched, because nothing ever physically moved.
        /// </summary>
        private async Task ReleaseReservedStock(Order order, string updatedById)
        {
            foreach (var orderItem in await orderItemsRepository.FindByOrderId(order.Id))
            {
                var inventory = await inventoryService.FindByProductVariantId(orderItem.ProductVariantId);
                if (inventory == null)
                {
                    continue;
                }

                await inventoryService.Update(new UpdateInventoryParams
                {
                    Id = inventory.Id,
                    ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - orderItem.Quantity),
                    UpdatedById = updatedById
                });
            }
        }

        private async Task ReserveStock(Guid productVariantId, int quantity, string updatedById)
        {
            var inventory = await FindInventoryWithStock(productVariantId, quantity);

            await inventoryService.Update(new UpdateInventoryParams
            {
                Id = inventory.Id,
                ReservedQuantity = inventory.ReservedQuantity + quantity,
                UpdatedById = updatedById
            });
        }

        private async Task<Order> FindOrThrow(Guid id)
        {
            return await ordersRepository.FindById(id)
                   ?? throw new ResourceNotFoundException("Order", $"Order with ID {id} not found");
        }

        private async Task<Inventory> FindInventoryWithStock(Guid productVariantId, int quantity)
        {
            var inventory = await inventoryService.FindByProductVariantId(productVariantId);

            // A variant nobody has stocked reads as out of stock rather than as a missing row.
            // The customer can do something about the first and nothing about the second.
            if (inventory == null)
            {
                throw new InsufficientStockException($"Product variant {productVariantId} is out of stock.");
            }

            return inventory.AvailableQuantity < quantity
                ? throw new InsufficientStockException(
                    $"Product variant {productVariantId} has {inventory.AvailableQuantity} unit(s) available but {quantity} were requested.")
                : inventory;
        }

        private async Task<ProductVariant> FindPurchasableVariant(Guid productVariantId)
        {
            var productVariant = await productVariantsRepository.FindById(productVariantId)
                                 ?? throw new ResourceNotFoundException("ProductVariant", $"Product variant with ID {productVariantId} not found");

            return !productVariant.IsActive
                ? throw new ProductVariantNotPurchasableException($"Product variant {productVariantId} is not active.")
                : productVariant;
        }

        private async Task<Product> FindProduct(Guid productId)
        {
            return await productsRepository.FindById(productId)
                   ?? throw new ResourceNotFoundException("Product", $"Product with ID {productId} not found");
        }

        /// <summary>
        /// Quotable by a customer and unique: the day it was placed plus a random tail. The
        /// column is unique, so a clash is re-drawn here rather than surfacing as a database
        /// error nobody can act on.
        /// </summary>
        private async Task<string> GenerateOrderNumber()
        {
            for (var attempt = 0; attempt < OrderNumberAttempts; attempt++)
            {
                var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

                if (await ordersRepository.FindByOrderNumber(orderNumber) == null)
                {
                    return orderNumber;
                }
            }

            throw new InvalidOperationException($"Could not generate a unique order number after {OrderNumberAttempts} attempts.");
        }

        private static void EnsureTransitionIsAllowed(OrderStatus current, OrderStatus next)
        {
            if (current == next)
            {
                throw new InvalidOrderStatusTransitionException($"Order is already {current}.");
            }

            var allowed = AllowedTransitions.TryGetValue(current, out var statuses) ? statuses : [];

            if (!allowed.Contains(next))
            {
                throw new InvalidOrderStatusTransitionException($"An order cannot move from {current} to {next}.");
            }
        }

        /// <summary>
        /// A last look at the money before it is written down. The discount now comes from a
        /// coupon, which already caps and clamps it, so these are no longer the first line of
        /// defence — they are the check that the number reaching the order is sane whatever
        /// produced it.
        /// </summary>
        private static void EnsureAmountsAreConsistent(decimal subtotal, decimal discountAmount, decimal shippingAmount)
        {
            if (discountAmount < 0)
            {
                throw new InvalidOrderAmountException("DiscountAmount cannot be negative.");
            }

            if (shippingAmount < 0)
            {
                throw new InvalidOrderAmountException("ShippingAmount cannot be negative.");
            }

            // Checked against the goods rather than the total: a discount larger than what is
            // being bought would bill the customer a negative amount.
            if (discountAmount > subtotal)
            {
                throw new InvalidOrderAmountException($"DiscountAmount ({discountAmount}) cannot be greater than the subtotal ({subtotal}).");
            }
        }
    }
}
