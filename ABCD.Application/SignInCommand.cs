using FluentValidation;

namespace ABCD.Application {
    public record SignInCommand {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }

    public class SignInCommandValidator : AbstractValidator<SignInCommand> {
        public SignInCommandValidator() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
