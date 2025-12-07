using FluentValidation;

namespace MediaTR.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Auth.Email.Required")
            .EmailAddress().WithMessage("Auth.Email.InvalidFormat")
            .MaximumLength(256).WithMessage("Auth.Email.TooLong");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Auth.Password.Required")
            .MinimumLength(8).WithMessage("Auth.Password.TooShort")
            .Matches(@"[A-Z]").WithMessage("Auth.Password.RequiresUppercase")
            .Matches(@"[a-z]").WithMessage("Auth.Password.RequiresLowercase")
            .Matches(@"[0-9]").WithMessage("Auth.Password.RequiresDigit")
            .Matches(@"[\W_]").WithMessage("Auth.Password.RequiresSpecialChar");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Auth.FirstName.Required")
            .MaximumLength(100).WithMessage("Auth.FirstName.TooLong");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Auth.LastName.Required")
            .MaximumLength(100).WithMessage("Auth.LastName.TooLong");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Auth.UserName.Required")
            .MinimumLength(3).WithMessage("Auth.UserName.TooShort")
            .MaximumLength(50).WithMessage("Auth.UserName.TooLong")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Auth.UserName.InvalidCharacters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Auth.PhoneNumber.InvalidFormat")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
