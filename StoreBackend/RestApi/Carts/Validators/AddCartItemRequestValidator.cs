using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Carts;

public class AddCartItemRequestValidator : BaseValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("ProductVariantId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
    }
}
