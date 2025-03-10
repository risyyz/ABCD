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
        public string SecurityAlgorithm { get => SecurityAlgorithms.HmacSha512; }

        public TokenValidationParameters GetTokenValidationParameters() {
            return new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                ValidateIssuerSigningKey = true,
                ClockSkew = System.TimeSpan.Zero
            };
        }
    }
}
