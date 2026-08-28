using Domain.Auth;  // AuthResult etc. live here
using Domain.Products;
using Domain.ProductImages;
using Domain.ProductVariants;
using Domain.Inventories;
using Domain.Categories;
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
