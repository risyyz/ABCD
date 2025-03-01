using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ABCD.Lib;

using FluentAssertions;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Moq;

namespace ABCD.Services.Tests {
    public class TokenServiceTests {

        private readonly Mock<JsonWebTokenHandler> _tokenHandlerMock;
        private readonly Mock<ISecurityTokenValidator> _tokenValidatorMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests() {
            _tokenHandlerMock = new Mock<JsonWebTokenHandler>();
            _tokenValidatorMock = new Mock<ISecurityTokenValidator>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();

            var jwtSettings = new JwtSettings {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SecretKey = "test-secret-key",
                TokenExpiryInMinutes = 60
            };

            _jwtSettingsMock.Setup(s => s.Value).Returns(jwtSettings);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: credentials);
            _tokenValidatorMock.Setup(x => x.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out securityToken)).Returns(new ClaimsPrincipal());

            _tokenService = new TokenService(_tokenHandlerMock.Object, _tokenValidatorMock.Object, _jwtSettingsMock.Object);
        }

        [Fact]
        public void GenerateToken_NullUser_ReturnsEmptyToken() {            
            // Act
            var token = _tokenService.GenerateToken(null);

            // Assert
            token.JWT.Should().BeEmpty();
            token.RefreshToken.Should().BeEmpty();
            token.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsValidToken() {
            // Arrange
            var user = new ApplicationUser {
                UserName = "username",
                Email = "user@example.com"
            };

            _tokenHandlerMock.Setup(x => x.CreateToken(It.IsAny<SecurityTokenDescriptor>())).Returns("test-token");

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            token.JWT.Should().Be("test-token");
            token.RefreshToken.Should().NotBeNullOrEmpty();
            token.IsEmpty.Should().BeFalse();
            _tokenHandlerMock.Verify(x => x.CreateToken(It.IsAny<SecurityTokenDescriptor>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetPrincipalFromToken_NullEmptyWhiteSpaceToken_ThrowsSecurityTokenException(string jwt) {
            // Act
            Action act = () => _tokenService.GetPrincipalFromToken(jwt);

            // Assert
            act.Should().Throw<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public void GetPrincipalFromToken_NullSecurityToken_ThrowsSecurityTokenException() {
            // Arrange
            var jwt = "invalid_jwt_token";

            SecurityToken securityToken;
            _tokenValidatorMock.Setup(x => x.ValidateToken(jwt, It.IsAny<TokenValidationParameters>(), out securityToken));

            // Act
            Action act = () => _tokenService.GetPrincipalFromToken(jwt);

            // Assert
            act.Should().Throw<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public void GetPrincipalFromToken_IncorrectSecurityAlgorithm_ThrowsSecurityTokenException() {
            // Arrange
            var jwt = "valid_jwt_token";

            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key")), "invalid-algorithm"));
            _tokenValidatorMock.Setup(x => x.ValidateToken(jwt, It.IsAny<TokenValidationParameters>(), out securityToken));

            // Act
            Action act = () => _tokenService.GetPrincipalFromToken(jwt);

            // Assert
            act.Should().Throw<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public void GetPrincipalFromToken_HmacSha256SecurityAlgorithm_ReturnsValidPrincipal() {
            // Arrange
            var jwt = "valid_jwt_token";

            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key")), SecurityAlgorithms.HmacSha256));
            _tokenValidatorMock.Setup(x => x.ValidateToken(jwt, It.IsAny<TokenValidationParameters>(), out securityToken)).Returns(
                new ClaimsPrincipal (
                    new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Name, "username"),
                        new Claim(ClaimTypes.Email, "user@email.com")
                })
            ));

            // Act
            var principal = _tokenService.GetPrincipalFromToken(jwt);

            // Assert
            principal.Should().NotBeNull();
            principal.Claims.Should().HaveCount(2);
            principal.Claims.Should().ContainSingle(x => x.Type == ClaimTypes.Name && x.Value == "username");
            principal.Claims.Should().ContainSingle(x => x.Type == ClaimTypes.Email && x.Value == "user@email.com");
        }
    }
}
