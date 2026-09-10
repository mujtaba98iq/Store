using Data;
using Data.Seeding;
using Microsoft.EntityFrameworkCore;
using RestApi.Configration;

namespace RestApi.Setup
{
    public static class DataSetup
    {
        public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
        {
            string connectionString = builder.Configuration[ConfigurationKeys.DatabaseConnectionString] 
                                      ?? throw new InvalidOperationException("Database connection string is not configured.");

            var settings = new DatabaseSettings { ConnectionString = connectionString };
            bool isDevelopment = builder.Environment.IsDevelopment();
            builder.Services.AddData(settings,isDevelopment);

            return builder;
        }

        public static void EnableAutoMigration(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }

        /// <summary>
        /// Inserts the starter catalogue. Idempotent - rows that already exist are
        /// left untouched, so it is safe to run on every start.
        /// </summary>
        public static void SeedCatalog(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            CatalogSeeder.SeedAsync(dbContext).GetAwaiter().GetResult();
        }

    }
}
