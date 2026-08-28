using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.ProductImages;

public class CreateProductImageRequestValidator : BaseValidator<CreateProductImageRequest>
{
    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image is required.")
            .SetValidator(new ImageFileValidator());

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder cannot be negative.")
            .When(x => x.DisplayOrder.HasValue);
    }
}
