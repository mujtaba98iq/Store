using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Auth;

public class RefreshRequestValidator : BaseValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Refresh token cannot be empty or whitespace.")
            .MinimumLength(10)
            .WithMessage("Refresh token must be at least 10 characters long.");

        RuleFor(x=> x.Email)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Email cannot be empty or whitespace.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
    }
}
