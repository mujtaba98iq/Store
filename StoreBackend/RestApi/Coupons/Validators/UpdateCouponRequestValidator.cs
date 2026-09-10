using Domain.Coupons;
using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Coupons;

public class UpdateCouponRequestValidator : BaseValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        // Each field is checked only when it was sent, an absent one keeping the value it
        // had. Nothing here compares two fields against each other: an update may carry one
        // half of a pair, and only the service — which has the stored coupon to merge
        // against — can tell what the other half will be.
        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("DiscountType must be either Percentage or FixedAmount.")
            .When(x => x.DiscountType.HasValue);

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("DiscountValue must be greater than zero.")
            .When(x => x.DiscountValue.HasValue);

        // Only when the type came with it. A value sent on its own may be landing on a
        // coupon that is already a fixed amount, where a hundred is no ceiling at all.
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(Coupon.MaxPercentageValue)
            .WithMessage($"A percentage coupon cannot exceed {Coupon.MaxPercentageValue}.")
            .When(x => x.DiscountValue.HasValue && x.DiscountType == DiscountType.Percentage);

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinimumOrderAmount cannot be negative.")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0)
            .WithMessage("MaximumDiscountAmount must be greater than zero.")
            .When(x => x.MaximumDiscountAmount.HasValue);

        // Both ends only, for the reason above: a request moving just the end date is
        // checked against the stored start date by the service.
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .WithMessage("EndDate must be after StartDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("UsageLimit must be greater than zero.")
            .When(x => x.UsageLimit.HasValue);
    }
}
