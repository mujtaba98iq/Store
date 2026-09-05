using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Orders;

public class UpdateOrderStatusRequestValidator : BaseValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        // Only checks that the value names a real status. Whether the order can actually
        // reach it from where it stands is a question about that order, so the service
        // answers it.
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of Pending, Confirmed, Processing, Shipped, Delivered or Cancelled.");
    }
}
