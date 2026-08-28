using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Inventories;

public class CreateInventoryRequestValidator : BaseValidator<CreateInventoryRequest>
{
    public CreateInventoryRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("ProductVariantId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.ReservedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ReservedQuantity cannot be negative.")
            .When(x => x.ReservedQuantity.HasValue);

        RuleFor(x => x.ReservedQuantity)
            .LessThanOrEqualTo(x => x.Quantity ?? 0)
            .WithMessage("ReservedQuantity cannot be greater than Quantity.")
            .When(x => x.ReservedQuantity.HasValue);
    }
}
