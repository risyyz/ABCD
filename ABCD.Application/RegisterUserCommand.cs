using System.Text.RegularExpressions;

using ABCD.Lib;

using FluentValidation;

namespace ABCD.Application {
    public record RegisterUserCommand {
        public required string Email { get; init; }
        public required string Password { get; init; }
        public required string PasswordConfirmation { get; init; }
    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand> {
        public RegisterUserCommandValidator(PasswordPolicy passwordPolicy) {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(passwordPolicy.MinPasswordLength).WithMessage($"Password must be at least {passwordPolicy.MinPasswordLength} characters long.")
                .Must(password => !passwordPolicy.RequireLowercase || Regex.IsMatch(password, "[a-z]"))
                .WithMessage("Password must contain at least one lowercase letter.")
                .Must(password => !passwordPolicy.RequireUppercase || Regex.IsMatch(password, "[A-Z]"))
                .WithMessage("Password must contain at least one uppercase letter.")
                .Must(password => !passwordPolicy.RequireDigit || Regex.IsMatch(password, "[0-9]"))
                .WithMessage("Password must contain at least one digit.")
                .Must(password => !passwordPolicy.RequireSpecialCharacter || Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                .WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.PasswordConfirmation)
                .Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}
