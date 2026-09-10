using Domain.Categories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Data.Seeding;

/// <summary>
/// Seeds a starter catalogue. Ids are fixed so the seeder is idempotent: rows
/// that already exist are left exactly as they are, including any edits made
/// through the API.
/// </summary>
public static class CatalogSeeder
{
    private const string SeedAuthor = "seed";

    private static readonly (Guid Id, string Name, string Description)[] Categories =
    [
        (Guid.Parse("2a1f0001-0000-4000-8000-000000000001"), "Cleansers", "Milks, gels and balms that lift the day off."),
        (Guid.Parse("2a1f0002-0000-4000-8000-000000000002"), "Toners", "Hydrating mists and essences to prep the skin."),
        (Guid.Parse("2a1f0003-0000-4000-8000-000000000003"), "Serums", "Targeted concentrates for tone and texture."),
        (Guid.Parse("2a1f0004-0000-4000-8000-000000000004"), "Moisturisers", "Barrier creams for day and night."),
        (Guid.Parse("2a1f0005-0000-4000-8000-000000000005"), "Oils", "Cold-pressed facial oils."),
        (Guid.Parse("2a1f0006-0000-4000-8000-000000000006"), "Masks", "Weekly treatments."),
        (Guid.Parse("2a1f0007-0000-4000-8000-000000000007"), "Sun Care", "Daily mineral protection."),
        (Guid.Parse("2a1f0008-0000-4000-8000-000000000008"), "Body", "Hands, elbows and everywhere else.")
    ];

    private static readonly (Guid Id, string Name, string Description, decimal Price, int Quantity, string ImagePath, Guid CategoryId)[] Products =
    [
        (Guid.Parse("3b2f0001-0000-4000-8000-000000000001"), "Moisture Repair Cream",
            "Rich barrier cream with squalane and oat lipids for overnight recovery.",
            67m, 24, "/product-images/product-cream.svg", Guid.Parse("2a1f0004-0000-4000-8000-000000000004")),
        (Guid.Parse("3b2f0002-0000-4000-8000-000000000002"), "Radiance Renewal Serum",
            "A daily vitamin C concentrate that evens tone and lifts dullness.",
            84m, 18, "/product-images/product-serum.svg", Guid.Parse("2a1f0003-0000-4000-8000-000000000003")),
        (Guid.Parse("3b2f0003-0000-4000-8000-000000000003"), "Gentle Milk Cleanser",
            "Fragrance-free milk that lifts sunscreen without stripping the barrier.",
            38m, 40, "/product-images/product-cleanser.svg", Guid.Parse("2a1f0001-0000-4000-8000-000000000001")),
        (Guid.Parse("3b2f0004-0000-4000-8000-000000000004"), "Rosewater Balancing Mist",
            "A hydrating veil of rosewater and glycerin to set skin before serums.",
            32m, 35, "/product-images/product-mist.svg", Guid.Parse("2a1f0002-0000-4000-8000-000000000002")),
        (Guid.Parse("3b2f0005-0000-4000-8000-000000000005"), "Nourishing Facial Oil",
            "Cold-pressed rosehip and jojoba to soften texture while you sleep.",
            72m, 15, "/product-images/product-oil.svg", Guid.Parse("2a1f0005-0000-4000-8000-000000000005")),
        (Guid.Parse("3b2f0006-0000-4000-8000-000000000006"), "Clay Clarifying Mask",
            "Kaolin and green tea draw out congestion in ten quiet minutes.",
            45m, 22, "/product-images/product-mask.svg", Guid.Parse("2a1f0006-0000-4000-8000-000000000006")),
        (Guid.Parse("3b2f0007-0000-4000-8000-000000000007"), "Smoothing Hand Balm",
            "Shea and beeswax salve that stays put through the day.",
            24m, 50, "/product-images/product-balm.svg", Guid.Parse("2a1f0008-0000-4000-8000-000000000008")),
        (Guid.Parse("3b2f0008-0000-4000-8000-000000000008"), "Daily Mineral SPF 40",
            "Weightless zinc protection with a soft, non-chalky finish.",
            54m, 30, "/product-images/product-spf.svg", Guid.Parse("2a1f0007-0000-4000-8000-000000000007")),
        (Guid.Parse("3b2f0009-0000-4000-8000-000000000009"), "Overnight Recovery Tube",
            "A ceramide-rich sleeping mask that seals in the rest of your routine.",
            59m, 20, "/product-images/product-tube.svg", Guid.Parse("2a1f0004-0000-4000-8000-000000000004"))
    ];

    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var seededAt = DateTime.UtcNow;

        var existingCategoryIds = await dbContext.Categories
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var newCategories = Categories
            .Where(c => !existingCategoryIds.Contains(c.Id))
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = seededAt,
                CreatedById = SeedAuthor
            })
            .ToList();

        if (newCategories.Count > 0)
        {
            dbContext.Categories.AddRange(newCategories);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existingProductIds = await dbContext.Products
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var missing = Products.Where(p => !existingProductIds.Contains(p.Id)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        // Attach the categories rather than re-inserting them.
        var categoryIds = missing.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await dbContext.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        foreach (var product in missing)
        {
            dbContext.Products.Add(new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                ImagePath = product.ImagePath,
                CreatedAt = seededAt,
                CreatedById = SeedAuthor,
                Categories = categories.TryGetValue(product.CategoryId, out var category)
                    ? [category]
                    : []
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
