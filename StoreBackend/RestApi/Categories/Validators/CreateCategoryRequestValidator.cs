using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Categories;

public class CreateCategoryRequestValidator : BaseValidator<CreateCategoryRequest>
{
    private const int NameMinLength = 3;
    private const int NameMaxLength = 100;
    private const int DescriptionMinLength = 3;
    private const int DescriptionMaxLength = 1000;

    public CreateCategoryRequestValidator()
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
            .MinimumLength(DescriptionMinLength)
            .WithMessage($"Description must be at least {DescriptionMinLength} characters long.")
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"Description cannot exceed {DescriptionMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
