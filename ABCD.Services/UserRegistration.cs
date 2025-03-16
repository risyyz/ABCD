using FluentValidation;

namespace ABCD.Services {
    public record UserRegistration {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirmation { get; init; }
    }

    public class UserRegistrationValidator : AbstractValidator<UserRegistration> {
        public UserRegistrationValidator() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.PasswordConfirmation)
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
