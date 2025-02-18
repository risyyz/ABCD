using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace ABCD.Lib {
    public class JwtSettings {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int TokenExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInMinutes { get; set; }

        public TokenValidationParameters GetTokenValidationParameters() {
            return new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
