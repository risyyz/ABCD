using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using ABCD.Lib;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

using Microsoft.IdentityModel.Tokens;

namespace ABCD.Services {

    public interface ITokenService {
        Token GenerateToken(ApplicationUser user);
        ClaimsPrincipal GetPrincipalFromToken(Token token);
    }

    public class TokenService : ITokenService {
        private readonly JsonWebTokenHandler _tokenHandler;
        private readonly ISecurityTokenValidator _tokenValidator;
        private readonly JwtSettings _jwtSettings;

        public TokenService(JsonWebTokenHandler tokenHandler, ISecurityTokenValidator tokenValidator, IOptions<JwtSettings> jwtSettings) {
            Guard.ThrowIfNull(tokenHandler, tokenValidator, jwtSettings);
            _tokenHandler = tokenHandler;
            _tokenValidator = tokenValidator;
            _jwtSettings = jwtSettings.Value;
        }

        public Token GenerateToken(ApplicationUser user) {
            if(user == null)
                return new Token { JWT = string.Empty, RefreshToken = string.Empty };  

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, "Admin") })
            };

            return new Token { JWT = _tokenHandler.CreateToken(tokenDescriptor), RefreshToken = GeneratRefreshToken() };
        }

        public ClaimsPrincipal GetPrincipalFromToken(Token token) {
            if(token?.IsEmpty ?? true)
                throw new SecurityTokenException("Invalid token");

            var principal = _tokenValidator.ValidateToken(token.JWT, _jwtSettings.GetTokenValidationParameters(), out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string GeneratRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    }
}
