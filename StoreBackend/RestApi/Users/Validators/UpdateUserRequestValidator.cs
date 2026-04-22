using FluentValidation;
using UseValidatorExtension.FluentValidation;

namespace RestApi.Users;

public class UpdateUserRequestValidator : BaseValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .MinimumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Username))
            .WithMessage("Username must be at least 3 characters long.");

        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("Password must be at least 6 characters long.");
    }
}
