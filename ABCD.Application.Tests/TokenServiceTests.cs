using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ABCD.Infra.Data;
using ABCD.Lib;

using FluentAssertions;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Moq;

namespace ABCD.Application.Tests {
    public class TokenServiceTests {

        private readonly Mock<SecurityTokenHandler> _tokenHandlerMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly TokenService _tokenService;
        private readonly string _validJwt = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoicmlzaGkudGFwc2VlQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiZTE2NjIzOTAtZjMyMi00ZTU2LTg4MDktZTgxYTdjZjhmYzg2IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NDE2NjM2NzAsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjcwMDEiLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdDo3MDAxIn0.OHqJ6XBZ4dyXQX7bQmfJkffARC5tjIBY9INoe-Cbawdz_9c7JvGk3aKQDPxWsCWB_1NAPf0KNuHYjl-vKNuKQA";

        public TokenServiceTests() {
            _tokenHandlerMock = new Mock<SecurityTokenHandler>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();

            var jwtSettings = new JwtSettings {
                Issuer = "test-issuer",
                Audience = "test-audience",
                SecretKey = "test-secret-key",
                TokenExpiryInMinutes = 60
            };

            _jwtSettingsMock.Setup(s => s.Value).Returns(jwtSettings);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: credentials);
            _tokenService = new TokenService(_tokenHandlerMock.Object, _jwtSettingsMock.Object);
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

            _tokenHandlerMock.Setup(x => x.WriteToken(It.IsAny<SecurityToken>())).Returns(_validJwt);

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            token.JWT.Should().Be(_validJwt);
            token.RefreshToken.Should().NotBeNullOrEmpty();
            token.IsEmpty.Should().BeFalse();
            _tokenHandlerMock.Verify(x => x.WriteToken(It.IsAny<SecurityToken>()), Times.Once);
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
            _tokenHandlerMock.Setup(x => x.ValidateToken(jwt, It.IsAny<TokenValidationParameters>(), out securityToken));

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
            _tokenHandlerMock.Setup(x => x.ValidateToken(jwt, It.IsAny<TokenValidationParameters>(), out securityToken));

            // Act
            Action act = () => _tokenService.GetPrincipalFromToken(jwt);

            // Assert
            act.Should().Throw<SecurityTokenException>().WithMessage("Invalid token");
        }

        [Fact]
        public void GetPrincipalFromToken_HmacSha256SecurityAlgorithm_ReturnsValidPrincipal() {

            SecurityToken securityToken = new JwtSecurityToken(signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-key")), SecurityAlgorithms.HmacSha512));
            _tokenHandlerMock.Setup(x => x.ValidateToken(_validJwt, It.IsAny<TokenValidationParameters>(), out securityToken)).Returns(
                new ClaimsPrincipal (
                    new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Name, "username"),
                        new Claim(ClaimTypes.Email, "user@email.com")
                })
            ));

            // Act
            var principal = _tokenService.GetPrincipalFromToken(_validJwt);

            // Assert
            principal.Should().NotBeNull();
            principal.Claims.Should().HaveCount(2);
            principal.Claims.Should().ContainSingle(x => x.Type == ClaimTypes.Name && x.Value == "username");
            principal.Claims.Should().ContainSingle(x => x.Type == ClaimTypes.Email && x.Value == "user@email.com");
        }
    }
}
