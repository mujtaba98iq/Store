using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.ProductImages;

public class UpdateProductImageRequestValidator : BaseValidator<UpdateProductImageRequest>
{
    public UpdateProductImageRequestValidator()
    {
        RuleFor(x => x.Image!)
            .SetValidator(new ImageFileValidator())
            .When(x => x.Image != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder cannot be negative.")
            .When(x => x.DisplayOrder.HasValue);
    }
}
