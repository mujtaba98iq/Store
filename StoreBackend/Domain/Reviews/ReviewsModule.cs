using Microsoft.Extensions.DependencyInjection;

namespace Domain.Reviews;

public static class ReviewsModule
{
    public static IServiceCollection AddReviewsModule(this IServiceCollection services)
    {
        services.AddScoped<IReviewService, ReviewService>();
        return services;
    }
}
