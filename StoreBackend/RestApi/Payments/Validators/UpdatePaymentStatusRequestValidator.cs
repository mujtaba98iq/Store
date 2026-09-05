using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Payments;

public class UpdatePaymentStatusRequestValidator : BaseValidator<UpdatePaymentStatusRequest>
{
    private const int TransactionIdMaxLength = 100;

    public UpdatePaymentStatusRequestValidator()
    {
        // Only checks that the value names a real status. Whether this payment can reach it
        // from where it stands, and whether a reference is needed to get there, are
        // questions about that payment, so the service answers them.
        RuleFor(x => x.PaymentStatus)
            .IsInEnum()
            .WithMessage("PaymentStatus must be one of Pending, Paid, Failed or Refunded.");

        RuleFor(x => x.TransactionId)
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("TransactionId cannot be empty or whitespace.")
            .MaximumLength(TransactionIdMaxLength)
            .WithMessage($"TransactionId cannot exceed {TransactionIdMaxLength} characters.")
            .When(x => x.TransactionId != null);
    }
}
