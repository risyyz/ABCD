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
        private readonly IAuthService _userService;
        private readonly IMapper _mapper;

        public AuthController( IAuthService userService, IMapper mapper) {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel loginRequest) {
            try {
                var userLogin = _mapper.Map<UserLogin>(loginRequest);
                var result = await _userService.LoginUser(userLogin);
                return Ok(new { result.Token, result.RefreshToken });
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));            
            } catch (LoginFailedException ex) {
                return Unauthorized("Invalid login attempt.");
            }            
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout() {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _userService.InvalidateToken(token);
            return Ok();
        }

        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestModel registerRequest) {
            try { 
                var userRegistration = _mapper.Map<UserRegistration>(registerRequest);
                await _userService.RegisterUser(userRegistration);
            }
            catch(ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            }

            return Ok("User registered successfully.");
        }
    }
}
