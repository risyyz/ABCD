using ABCD.Application;
using ABCD.Application.Exceptions;
using ABCD.Lib;
using ABCD.Server.Models;
using ABCD.Server.Requests;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;
        private readonly ITypeMapper _mapper;

        public AuthController(IAuthService authService, ITypeMapper mapper) {
            _authService = authService;
            _mapper = mapper;
        }        

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInRequest signInRequest) {
            try {
                var credentials = _mapper.Map<SignInRequest, SignInCommand>(signInRequest);
                var result = await _authService.SignIn(credentials);
                UpdateTokenCookies(result.JWT, result.RefreshToken, 60, 60);
                return Ok(new { success = true });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            } catch (SignInFailedException) {
                return Unauthorized("Invalid login attempt.");
            }
        }

        [Authorize]
        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut([FromServices] BearerTokenReader tokenReader) {
            var token = tokenReader.GetAccessToken();
            if (string.IsNullOrWhiteSpace(token)) {
                return Unauthorized("Authorization token is missing or invalid.");
            }

            await _authService.SignOut(token);

            // Clear authentication cookies
            Response.Cookies.Delete(AppConstants.ACCESS_TOKEN, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            Response.Cookies.Delete(AppConstants.REFRESH_TOKEN, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok();
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromServices] BearerTokenReader tokenReader, RefreshTokenRequest refreshTokenRequest) {
            try {
                var accessToken = tokenReader.GetAccessToken();
                if (string.IsNullOrWhiteSpace(accessToken))
                    return Unauthorized("Authorization token is missing or invalid.");

                // Read refresh token from cookie
                var refreshToken = tokenReader.GetRefreshToken();
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return Unauthorized("Refresh token is missing or invalid.");

                var tokenRefreshCommand = new RefreshTokenCommand {
                    Email = refreshTokenRequest.Email,
                    RefreshToken = refreshToken,
                    JWT = accessToken
                };

                var result = await _authService.RefreshToken(tokenRefreshCommand);
                UpdateTokenCookies(result.JWT, result.RefreshToken, 30, 60);

                return Ok(new { success = true });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            } catch (SecurityTokenException ex) {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetAuthenticationStatus() {
            // If we reach here, the user is authenticated (JWT from cookie is valid)
            var email = User.Identity?.Name;
            return Ok(new { isAuthenticated = true, email });
        }

        private void UpdateTokenCookies(string jwt, string refreshToken, int jwtExpiryMinutes, int refreshTokenExpiryMinutes) {
            Response.Cookies.Append(AppConstants.ACCESS_TOKEN, jwt, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(jwtExpiryMinutes)
            });

            Response.Cookies.Append(AppConstants.REFRESH_TOKEN, refreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(refreshTokenExpiryMinutes)
            });
        }
    }
}
