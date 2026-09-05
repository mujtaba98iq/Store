using Domain.Payments;
using RestApi.Payments;

namespace RestApi.Setup;

public static class PaymentsSetup
{
    public static WebApplicationBuilder AddPaymentsModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddPaymentsModule();
        builder.Services.AddScoped<IPaymentResponseFormatter, PaymentResponseFormatter>();
        return builder;
    }
}
