using Domain.Reviews;
using RestApi.Reviews;

namespace RestApi.Setup;

public static class ReviewsSetup
{
    public static WebApplicationBuilder AddReviewsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddReviewsModule();
        builder.Services.AddScoped<IReviewResponseFormatter, ReviewResponseFormatter>();
        return builder;
    }
}
