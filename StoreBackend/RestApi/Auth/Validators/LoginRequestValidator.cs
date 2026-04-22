using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Auth;

public class LoginRequestValidator : BaseValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Username cannot be empty or whitespace.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.");

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Password cannot be empty or whitespace.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");
    }
}
