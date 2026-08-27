using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.ProductImages;

public class CreateProductImageRequestValidator : BaseValidator<CreateProductImageRequest>
{
    private const int ImageUrlMaxLength = 2048;

    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.ImageUrl)
            .NotNull()
            .NotEmpty()
            .Must(u => !string.IsNullOrWhiteSpace(u))
            .WithMessage("ImageUrl cannot be empty or whitespace.")
            .MaximumLength(ImageUrlMaxLength)
            .WithMessage($"ImageUrl cannot exceed {ImageUrlMaxLength} characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder cannot be negative.")
            .When(x => x.DisplayOrder.HasValue);
    }
}
