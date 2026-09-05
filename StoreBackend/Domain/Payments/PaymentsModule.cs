using Microsoft.Extensions.DependencyInjection;

namespace Domain.Payments;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
