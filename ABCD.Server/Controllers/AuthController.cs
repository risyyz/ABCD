using ABCD.Server.RequestModels;
using ABCD.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly ITokenService tokenService;
        private readonly IUserService userService;

        public AuthController(ITokenService tokenService, IUserService userService) {
            this.tokenService = tokenService;
            this.userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel loginRequest) {
            var token = await tokenService.GenerateToken();
            return Ok(token);
        }

        [Authorize]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestModel registerRequest) {
            if (registerRequest.Password != registerRequest.ConfirmPassword) {
                return BadRequest("Passwords do not match.");
            }

            var result = await userService.RegisterUser(registerRequest.Email, registerRequest.Password);
            if (!result.Succeeded) {
                return BadRequest(result.Errors);
            }

            return Ok("User registered successfully.");
        }
    }
}

