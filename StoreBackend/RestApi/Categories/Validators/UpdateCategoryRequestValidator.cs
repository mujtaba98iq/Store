using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Categories;

public class UpdateCategoryRequestValidator : BaseValidator<UpdateCategoryRequest>
{
    private const int NameMinLength = 3;
    private const int NameMaxLength = 100;
    private const int DescriptionMinLength = 3;
    private const int DescriptionMaxLength = 1000;

    public UpdateCategoryRequestValidator()
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
    }
}
