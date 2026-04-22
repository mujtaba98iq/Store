using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Products;

public class CreateProductRequestValidator : BaseValidator<CreateProductRequest>
{
    private const int NameMinLength = 5;
    private const int NameMaxLength = 100;
    private const int DescriptionMinLength = 5;
    private const int DescriptionMaxLength = 1000;

    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Name cannot be empty or whitespace.")
            .MinimumLength(NameMinLength)
            .WithMessage($"Name must be at least {NameMinLength} characters long.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"Name cannot exceed {NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Description cannot be empty or whitespace.")
            .MinimumLength(DescriptionMinLength)
            .WithMessage($"Description must be at least {DescriptionMinLength} characters long.")
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Description cannot exceed {DescriptionMaxLength} characters.");

        RuleFor(x => x.Price)
            .NotNull()
            .WithMessage("Price is required.")
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Quantity)
            .NotNull()
            .WithMessage("Quantity is required.")
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.");

        RuleFor(x => x.ImagePath)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("ImagePath cannot be empty.")
            .MaximumLength(500)
            .WithMessage("ImagePath is too long.");
    }
}