using RestApi.Setup;
using RestApi.Users.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);



builder.AddData();
builder.AddProductsModule();
builder.AddCategoriesModule();
builder.AddUsersModule();
builder.AddAuth();



builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StoreApiCorsPolicy",policy =>
    {
        policy.WithOrigins("http://localhost:5127", "https://localhost:7130")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("StoreApiCorsPolicy");

app.UseRateLimiter();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
    {
        await context.Response.WriteAsync("Too many login attempts. Please try again later.");
    }
});

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapOpenApi();

app.EnableAutoMigration();



app.Run();