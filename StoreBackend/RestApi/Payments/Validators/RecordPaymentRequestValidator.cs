using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Payments;

public class RecordPaymentRequestValidator : BaseValidator<RecordPaymentRequest>
{
    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("PaymentMethod must be one of CashOnDelivery, Card, ZainCash, FastPay or BankTransfer.");
    }
}
