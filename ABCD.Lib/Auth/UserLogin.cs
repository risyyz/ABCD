using FluentValidation;

namespace ABCD.Lib.Auth {
    public record UserLogin {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }

    public class UserLoginValidator : AbstractValidator<UserLogin> {
        public UserLoginValidator() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
