using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Shipments;

public class CreateShipmentRequestValidator : BaseValidator<CreateShipmentRequest>
{
    private const int TextMaxLength = 100;

    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        // Both are optional, but a value that was sent has to be a real one: a blank
        // tracking number reads as tracked when it is not.
        RuleFor(x => x.TrackingNumber)
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("TrackingNumber cannot be empty or whitespace.")
            .MaximumLength(TextMaxLength)
            .WithMessage($"TrackingNumber cannot exceed {TextMaxLength} characters.")
            .When(x => x.TrackingNumber != null);

        RuleFor(x => x.ShippingProvider)
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("ShippingProvider cannot be empty or whitespace.")
            .MaximumLength(TextMaxLength)
            .WithMessage($"ShippingProvider cannot exceed {TextMaxLength} characters.")
            .When(x => x.ShippingProvider != null);
    }
}
