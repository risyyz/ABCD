using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using ABCD.Lib;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Application {

    public interface ITokenService {
        Token GenerateToken(ApplicationUser user);
        ClaimsPrincipal GetPrincipalFromToken(string jwt);
    }

    public class TokenService : ITokenService {        
        private readonly SecurityTokenHandler _securityTokenHandler;
        private readonly JwtSettings _jwtSettings;

        public TokenService(SecurityTokenHandler securityTokenHandler, IOptions<JwtSettings> jwtSettings) {
            Guard.ThrowIfNull(securityTokenHandler, jwtSettings);
            _securityTokenHandler = securityTokenHandler;
            _securityTokenHandler = securityTokenHandler;
            _jwtSettings = jwtSettings.Value;
        }

        public Token GenerateToken(ApplicationUser user) {
            if(user == null)
                return new Token { JWT = string.Empty, RefreshToken = string.Empty };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, _jwtSettings.SecurityAlgorithm);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                claims:  claims
            );
            return new Token { JWT = _securityTokenHandler.WriteToken(tokenDescriptor), RefreshToken = GeneratRefreshToken() };
        }

        public ClaimsPrincipal GetPrincipalFromToken(string jwt) {
            if (string.IsNullOrWhiteSpace(jwt))
                throw new SecurityTokenException("Invalid token");

            var principal = _securityTokenHandler.ValidateToken(jwt, _jwtSettings.GetTokenValidationParameters(), out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(_jwtSettings.SecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase)) {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private string GeneratRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    }
}
