using ABCD.Server.RequestModels;
using ABCD.Services.Security;

using Microsoft.AspNetCore.Mvc;

namespace ABCD.Server.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly ITokenService tokenService;

        public AuthController(ITokenService tokenService) {
            this.tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel loginRequest) {    
            var token = await tokenService.GenerateToken();
            return Ok(token);
        }
    }
}
