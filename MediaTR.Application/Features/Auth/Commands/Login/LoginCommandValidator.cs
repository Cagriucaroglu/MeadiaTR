using FluentValidation;

namespace MediaTR.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Auth.EmailOrUsername.Required")
            .MaximumLength(256).WithMessage("Auth.EmailOrUsername.TooLong");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Auth.Password.Required");
    }
}
