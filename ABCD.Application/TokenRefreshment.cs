using FluentValidation;

namespace ABCD.Application {
    public record TokenRefreshment {
        public required string Email { get; init; }
        public required string JWT { get; init; }
        public required string RefreshToken { get; init; }
    }

    public class TokenRefreshmentValidator : AbstractValidator<TokenRefreshment> {
        public TokenRefreshmentValidator() {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.JWT)
                .NotEmpty().WithMessage("JWT is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken is required");
        }
    }
}
