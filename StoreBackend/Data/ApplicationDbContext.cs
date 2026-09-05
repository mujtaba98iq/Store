using Domain.Auth;  // AuthResult etc. live here
using Domain.Products;
using Domain.ProductImages;
using Domain.ProductVariants;
using Domain.Inventories;
using Domain.Categories;
using Domain.Carts;
using Domain.Orders;
using Domain.Payments;
using Domain.Shipments;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
namespace Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderShippingAddress> OrderShippingAddresses { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.Description)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.ImagePath)
            .IsRequired();

        modelBuilder.Entity<ProductVariant>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<ProductVariant>()
            .Property(v => v.Sku)
            .IsRequired();

        modelBuilder.Entity<ProductVariant>()
            .HasIndex(v => v.Sku)
            .IsUnique();

        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inventory>()
            .HasKey(i => i.Id);

        // Derived from Quantity and ReservedQuantity, so it must never become a column.
        modelBuilder.Entity<Inventory>()
            .Ignore(i => i.AvailableQuantity);

        // One stock row per variant, otherwise the same units could be counted twice.
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => i.ProductVariantId)
            .IsUnique();

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.ProductVariant)
            .WithOne(v => v.Inventory)
            .HasForeignKey<Inventory>(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductImage>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<ProductImage>()
            .Property(i => i.ImageUrl)
            .IsRequired();

        modelBuilder.Entity<ProductImage>()
            .HasIndex(i => new { i.ProductId, i.DisplayOrder });

        modelBuilder.Entity<ProductImage>()
            .HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<Product>()
            .HasMany(p => p.Categories)
            .WithMany(c => c.Products)
            .UsingEntity(j => j.ToTable("ProductCategories"));

        modelBuilder.Entity<Cart>()
            .HasKey(c => c.Id);

        // Summed from the lines it holds, so it must never become a column.
        modelBuilder.Entity<Cart>()
            .Ignore(c => c.TotalAmount);

        // A customer shops out of a single cart, two of them would split the same basket.
        // Filtered so the constraint ignores carts that were already closed.
        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasKey(i => i.Id);

        // Quantity * UnitPrice, so it must never become a column.
        modelBuilder.Entity<CartItem>()
            .Ignore(i => i.Subtotal);

        // A variant is topped up on the line it already has rather than listed twice.
        // Filtered so it can be added again after its line was taken out.
        modelBuilder.Entity<CartItem>()
            .HasIndex(i => new { i.CartId, i.ProductVariantId })
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderNumber)
            .IsRequired();

        // Customers quote this instead of the id, and support looks orders up by it, so it
        // has to point at exactly one order.
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        // The common read is a customer opening their own order history, newest first.
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.CreatedAt });

        // Restricted rather than cascaded, unlike a cart: an order is a financial record and
        // must outlive the account it was placed from.
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasKey(i => i.Id);

        // Copies of the catalogue as it read at checkout, not lookups. They are required
        // because a line that cannot say what was bought is not worth keeping.
        modelBuilder.Entity<OrderItem>()
            .Property(i => i.ProductName)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.Sku)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .HasIndex(i => i.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restricted so retiring a variant cannot erase what somebody already bought.
        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderShippingAddress>()
            .HasKey(a => a.Id);

        // A copy of the address as it read at checkout, not a lookup. Required because an
        // order nobody can deliver, or later prove was delivered, is not worth keeping.
        // Area is the one part left optional: not every address is given with a district.
        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.FullName)
            .IsRequired();

        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.PhoneNumber)
            .IsRequired();

        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.Country)
            .IsRequired();

        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.City)
            .IsRequired();

        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.Street)
            .IsRequired();

        modelBuilder.Entity<OrderShippingAddress>()
            .Property(a => a.Building)
            .IsRequired();

        // One address per order, which the one-to-one enforces with a unique key on OrderId:
        // an order holding two of them could not say where the parcel actually went.
        // Cascaded, unlike the order's own link to the user, because the address is part of
        // the order rather than a record in its own right.
        modelBuilder.Entity<OrderShippingAddress>()
            .HasOne(a => a.Order)
            .WithOne(o => o.ShippingAddress)
            .HasForeignKey<OrderShippingAddress>(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasKey(p => p.Id);

        // Not unique: an order can hold several payments, because a decline followed by a
        // retry, or a settlement followed by a refund, are all part of how it was paid. The
        // read this serves is always "the payments against this order".
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.OrderId);

        // A provider's transaction reference belongs to one payment and no other, so a
        // callback that arrives twice cannot be banked twice. Filtered because most rows
        // legitimately have none: cash on delivery never reaches a provider, and neither
        // does an attempt that failed before it got there.
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId)
            .IsUnique()
            .HasFilter("\"TransactionId\" IS NOT NULL");

        // Cascaded, as the order's own lines are: a payment settles one order and says
        // nothing without it.
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shipment>()
            .HasKey(s => s.Id);

        // Customers and support both chase a parcel by the number the carrier gave them.
        // Not unique: carriers number their own consignments and two of them may well
        // arrive at the same string.
        modelBuilder.Entity<Shipment>()
            .HasIndex(s => s.TrackingNumber);

        // One shipment per order, which the one-to-one enforces with a unique key on
        // OrderId: two of them could not agree on where the order has got to. Cascaded for
        // the same reason as the shipping address, the shipment being part of the order
        // rather than a record in its own right.
        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.Order)
            .WithOne(o => o.Shipment)
            .HasForeignKey<Shipment>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired();

    }
}
