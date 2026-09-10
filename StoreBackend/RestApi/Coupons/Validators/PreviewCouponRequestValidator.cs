using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Coupons;

public class PreviewCouponRequestValidator : BaseValidator<PreviewCouponRequest>
{
    public PreviewCouponRequestValidator()
    {
        // Only the shape of the code. Whether it names a live campaign the caller's cart
        // qualifies for is the whole question the endpoint exists to answer, so the service
        // answers it.
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required.")
            .MaximumLength(CouponValidation.CodeMaxLength)
            .WithMessage($"Code cannot exceed {CouponValidation.CodeMaxLength} characters.");
    }
}
