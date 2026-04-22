using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Products;

public class UpdateProductRequestValidator : BaseValidator<UpdateProductRequest>
{
    private const int NameMinLength = 5;
    private const int NameMaxLength = 100;
    private const int DescriptionMinLength = 5;
    private const int DescriptionMaxLength = 1000;

    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(NameMinLength)
            .WithMessage($"Name must be at least {NameMinLength} characters long.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name cannot exceed {NameMaxLength} characters.")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MinimumLength(DescriptionMinLength)
            .WithMessage($"Description must be at least {DescriptionMinLength} characters long.")
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Description cannot exceed {DescriptionMaxLength} characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.ImagePath)
            .MaximumLength(500)
            .WithMessage("ImagePath is too long.")
            .When(x => x.ImagePath != null);
    }
}