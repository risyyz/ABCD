using ABCD.Lib;
using ABCD.Lib.Exceptions;
using ABCD.Server.Requests;
using ABCD.Application;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ABCD.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;
        private readonly IClassMapper _mapper;

        public AuthController(IAuthService authService, IClassMapper mapper) {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInRequest signInRequest) {
            try {
                var credentials = _mapper.Map<SignInRequest, SignInCredentials>(signInRequest);
                var result = await _authService.SignIn(credentials);
                
                // Set JWT token as HTTP-only cookie
                Response.Cookies.Append("access_token", result.JWT, new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30) // Match token expiry
                });
                
                // Set refresh token as HTTP-only cookie
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60) // Match refresh token expiry
                });
                
                return Ok(new { success = true });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            } catch (SignInFailedException ex) {
                return Unauthorized("Invalid login attempt.");
            }
        }

        [Authorize]
        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut([FromServices] BearerTokenReader tokenReader) {
            var token = tokenReader.GetToken();
            if (string.IsNullOrEmpty(token)) {
                return Unauthorized("Authorization token is missing or invalid.");
            }

            await _authService.SignOut(token);
            
            // Clear authentication cookies
            Response.Cookies.Delete("access_token", new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            Response.Cookies.Delete("refresh_token", new CookieOptions {
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
                var token = tokenReader.GetToken();
                if (string.IsNullOrEmpty(token)) {
                    return Unauthorized("Authorization token is missing or invalid.");
                }
                
                // Read refresh token from cookie
                var refreshTokenFromCookie = Request.Cookies["refresh_token"];
                if (string.IsNullOrEmpty(refreshTokenFromCookie)) {
                    return Unauthorized("Refresh token is missing.");
                }

                var refreshment = new TokenRefreshment {
                    Email = refreshTokenRequest.Email,
                    RefreshToken = refreshTokenFromCookie,
                    JWT = token
                };
                var result = await _authService.RefreshToken(refreshment);
                
                // Set new JWT token as HTTP-only cookie
                Response.Cookies.Append("access_token", result.JWT, new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                });
                
                // Set new refresh token as HTTP-only cookie
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });
                
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
    }
}
