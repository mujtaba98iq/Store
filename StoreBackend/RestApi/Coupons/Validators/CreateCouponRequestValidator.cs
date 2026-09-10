using Domain.Coupons;
using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Coupons;

public class CreateCouponRequestValidator : BaseValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required.")
            .MinimumLength(CouponValidation.CodeMinLength)
            .WithMessage($"Code must be at least {CouponValidation.CodeMinLength} characters.")
            .MaximumLength(CouponValidation.CodeMaxLength)
            .WithMessage($"Code cannot exceed {CouponValidation.CodeMaxLength} characters.")
            .Matches(CouponValidation.CodePattern)
            .WithMessage(CouponValidation.CodePatternMessage);

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("DiscountType must be either Percentage or FixedAmount.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("DiscountValue must be greater than zero.");

        // The ceiling on a percentage is checked here as well as in the service, because
        // create carries every field and so can be judged whole. The service still owns the
        // rule: this is the early, readable refusal, not the authority.
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(Coupon.MaxPercentageValue)
            .WithMessage($"A percentage coupon cannot exceed {Coupon.MaxPercentageValue}.")
            .When(x => x.DiscountType == DiscountType.Percentage);

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinimumOrderAmount cannot be negative.")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0)
            .WithMessage("MaximumDiscountAmount must be greater than zero.")
            .When(x => x.MaximumDiscountAmount.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("UsageLimit must be greater than zero. Leave it out for an unlimited coupon.")
            .When(x => x.UsageLimit.HasValue);
    }
}
