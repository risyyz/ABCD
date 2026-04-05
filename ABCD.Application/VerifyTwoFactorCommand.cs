using FluentValidation;

namespace ABCD.Application {
    public record VerifyTwoFactorCommand {
        public required string Email { get; init; }
        public required string Pin { get; init; }
    }

    public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand> {
        public VerifyTwoFactorCommandValidator() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Pin)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(6).WithMessage("Verification code must be 6 digits.")
                .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");
        }
    }
}
