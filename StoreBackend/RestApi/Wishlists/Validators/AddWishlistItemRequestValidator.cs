using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Wishlists;

public class AddWishlistItemRequestValidator : BaseValidator<AddWishlistItemRequest>
{
    public AddWishlistItemRequestValidator()
    {
        // The only thing there is to check. Whether the product exists is a question about
        // the catalogue, so the service answers it.
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");
    }
}
