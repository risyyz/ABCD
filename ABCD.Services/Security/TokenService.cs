using System.Security.Claims;
using System.Text;

using ABCD.Lib;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Services.Security {
    public class TokenService : ITokenService {

        private readonly JwtSettings jwtSettings;
        private readonly JsonWebTokenHandler tokenHandler;

        public TokenService(IOptions<JwtSettings> jwtSettings, JsonWebTokenHandler tokenHandler) {
            this.jwtSettings = jwtSettings.Value;
            this.tokenHandler = tokenHandler;
        }

        public async Task<string> GenerateToken() {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddMinutes(jwtSettings.ExpiryInMinutes),
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, "John Doe"),
                    new Claim(ClaimTypes.Email, "john.doe@abcd.com"),
                    new Claim(ClaimTypes.Role, "Admin") })
            };

            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
