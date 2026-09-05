using System.Linq.Expressions;
using FluentValidation;

namespace RestApi.Orders;

/// <summary>
/// Everything the courier needs is required. Area is the exception: not every address is
/// given with a district, and refusing those would block real deliveries.
/// </summary>
public class ShippingAddressRequestValidator : AbstractValidator<ShippingAddressRequest>
{
    private const int PhoneNumberMinLength = 7;
    private const int PhoneNumberMaxLength = 20;
    private const int TextMaxLength = 100;

    public ShippingAddressRequestValidator()
    {
        RequireText(x => x.FullName, "FullName");
        RequireText(x => x.Country, "Country");
        RequireText(x => x.City, "City");
        RequireText(x => x.Street, "Street");
        RequireText(x => x.Building, "Building");

        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("PhoneNumber cannot be empty or whitespace.")
            .MinimumLength(PhoneNumberMinLength)
            .WithMessage($"PhoneNumber must be at least {PhoneNumberMinLength} characters long.")
            .MaximumLength(PhoneNumberMaxLength)
            .WithMessage($"PhoneNumber cannot exceed {PhoneNumberMaxLength} characters.");

        RuleFor(x => x.Area)
            .MaximumLength(TextMaxLength)
            .WithMessage($"Area cannot exceed {TextMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Area));
    }

    private void RequireText(Expression<Func<ShippingAddressRequest, string>> selector, string name)
    {
        RuleFor(selector)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage($"{name} cannot be empty or whitespace.")
            .MaximumLength(TextMaxLength)
            .WithMessage($"{name} cannot exceed {TextMaxLength} characters.");
    }
}
