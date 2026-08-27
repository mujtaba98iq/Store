using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.ProductVariants;

public class UpdateProductVariantRequestValidator : BaseValidator<UpdateProductVariantRequest>
{
    private const int SkuMinLength = 3;
    private const int SkuMaxLength = 50;
    private const int BarcodeMinLength = 6;
    private const int BarcodeMaxLength = 50;

    public UpdateProductVariantRequestValidator()
    {
        RuleFor(x => x.Sku)
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Sku cannot be empty or whitespace.")
            .MinimumLength(SkuMinLength)
            .WithMessage($"Sku must be at least {SkuMinLength} characters long.")
            .MaximumLength(SkuMaxLength)
            .WithMessage($"Sku cannot exceed {SkuMaxLength} characters.")
            .When(x => x.Sku != null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Barcode)
            .MinimumLength(BarcodeMinLength)
            .WithMessage($"Barcode must be at least {BarcodeMinLength} characters long.")
            .MaximumLength(BarcodeMaxLength)
            .WithMessage($"Barcode cannot exceed {BarcodeMaxLength} characters.")
            .When(x => x.Barcode != null);
    }
}
