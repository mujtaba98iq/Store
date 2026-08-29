using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Carts;

public class UpdateCartItemRequestValidator : BaseValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        // Zero is rejected on purpose: emptying a line is what DELETE is for.
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
    }
}
