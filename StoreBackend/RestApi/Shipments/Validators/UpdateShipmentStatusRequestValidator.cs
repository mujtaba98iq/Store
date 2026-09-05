using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Shipments;

public class UpdateShipmentStatusRequestValidator : BaseValidator<UpdateShipmentStatusRequest>
{
    public UpdateShipmentStatusRequestValidator()
    {
        // Only checks that the value names a real status. Whether this parcel can reach it
        // from where it stands is a question about that parcel, so the service answers it.
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of Pending, Preparing, Shipped, OutForDelivery, Delivered or Returned.");
    }
}
