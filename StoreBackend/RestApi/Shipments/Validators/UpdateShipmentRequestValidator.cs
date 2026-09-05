using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Shipments;

public class UpdateShipmentRequestValidator : BaseValidator<UpdateShipmentRequest>
{
    private const int TextMaxLength = 100;

    public UpdateShipmentRequestValidator()
    {
        // Null means "leave it alone", so only a value that was actually sent is checked.
        // Blank is refused rather than treated as a clear: erasing a carrier by sending an
        // empty string is too easy to do by accident.
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
