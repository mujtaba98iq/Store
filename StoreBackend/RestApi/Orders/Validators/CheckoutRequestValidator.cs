using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Orders;

public class CheckoutRequestValidator : BaseValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DiscountAmount cannot be negative.");

        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ShippingAmount cannot be negative.");

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("ShippingAddress is required.")
            .SetValidator(new ShippingAddressRequestValidator());
    }
}
