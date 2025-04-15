using ABCD.Lib;
using ABCD.Lib.Exceptions;
using ABCD.Server.Requests;
using ABCD.Services;

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

        public AuthController( IAuthService authService, IClassMapper mapper) {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(SignInRequest signInRequest) {
            try {
                var credentials = _mapper.Map<SignInRequest, SignInCredentials>(signInRequest);
                var result = await _authService.SignIn(credentials);
                return Ok(new { token = result.JWT, refreshToken = result.RefreshToken });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));            
            } catch (SignInFailedException ex) {
                return Unauthorized("Invalid login attempt.");
            }            
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOut() {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _authService.SignOut(token);
            return Ok();
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest) {
            try {
                var refreshment = new TokenRefreshment {
                    Email = refreshTokenRequest.Email,
                    RefreshToken = refreshTokenRequest.RefreshToken,
                    JWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "")
                };
                var result = await _authService.RefreshToken(refreshment);
                return Ok(new { token = result.JWT, refreshToken = result.RefreshToken });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));            
            } catch (SecurityTokenException ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
