using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.ProductImages;

public class UpdateProductImageRequestValidator : BaseValidator<UpdateProductImageRequest>
{
    private const int ImageUrlMaxLength = 2048;

    public UpdateProductImageRequestValidator()
    {
        RuleFor(x => x.ImageUrl)
            .Must(u => !string.IsNullOrWhiteSpace(u))
            .WithMessage("ImageUrl cannot be empty or whitespace.")
            .MaximumLength(ImageUrlMaxLength)
            .WithMessage($"ImageUrl cannot exceed {ImageUrlMaxLength} characters.")
            .When(x => x.ImageUrl != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder cannot be negative.")
            .When(x => x.DisplayOrder.HasValue);
    }
}
