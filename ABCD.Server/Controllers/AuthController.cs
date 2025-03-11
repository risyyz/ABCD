using ABCD.Lib.Auth;
using ABCD.Lib.Exceptions;
using ABCD.Server.RequestModels;
using ABCD.Services;

using AutoMapper;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController( IAuthService authService, IMapper mapper) {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(SignInRequestModel signInRequest) {
            try {
                var credentials = _mapper.Map<SignInCredentials>(signInRequest);
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
        public async Task<IActionResult> RefreshToken() {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _authService.SignOut(token);
            return Ok();
        }
    }
}
