using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Inventories;

public class UpdateInventoryRequestValidator : BaseValidator<UpdateInventoryRequest>
{
    public UpdateInventoryRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.ReservedQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ReservedQuantity cannot be negative.")
            .When(x => x.ReservedQuantity.HasValue);

        // Only checkable here when both values are sent together. A partial update is
        // validated against the stored row by InventoryService.
        RuleFor(x => x.ReservedQuantity)
            .LessThanOrEqualTo(x => x.Quantity!.Value)
            .WithMessage("ReservedQuantity cannot be greater than Quantity.")
            .When(x => x.ReservedQuantity.HasValue && x.Quantity.HasValue);
    }
}
