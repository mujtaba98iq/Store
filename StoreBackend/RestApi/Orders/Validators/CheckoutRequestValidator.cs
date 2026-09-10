using FluentValidation;
using RestApi.Coupons;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Orders;

public class CheckoutRequestValidator : BaseValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        // Length only, and only when a code was sent at all: an empty box is a customer who
        // did not use a coupon. Whether the code names a live campaign the basket qualifies
        // for is a question about the coupon, so the service answers it.
        RuleFor(x => x.CouponCode)
            .MaximumLength(CouponValidation.CodeMaxLength)
            .WithMessage($"CouponCode cannot exceed {CouponValidation.CodeMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CouponCode));

        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ShippingAmount cannot be negative.");

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("ShippingAddress is required.")
            .SetValidator(new ShippingAddressRequestValidator());

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("PaymentMethod must be one of CashOnDelivery, Card, ZainCash, FastPay or BankTransfer.");
    }
}
