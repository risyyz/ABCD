using ABCD.Lib;
using ABCD.Application;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ABCD.Server.Requests;
using FluentValidation;

namespace ABCD.Server.Controllers {

    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly IUserService _userService;
        private readonly ITypeMapper _mapper;
        public UsersController(IUserService userService, ITypeMapper mapper) {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers() {
            var users = await _userService.GetUsers();
            return Ok(users);
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetUser(string id) {
        //    var user = await _userService.GetUser(id);
        //    return Ok(user);
        //}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateUser(string id, UpdateUserRequestModel updateUserRequest) {
        //    try {
        //        var user = _mapper.Map<UpdateUser>(updateUserRequest);
        //        await _userService.UpdateUser(id, user);
        //        return Ok("User updated successfully.");
        //    } catch (ValidationException ex) {
        //        return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage));
        //    }
        //}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(string id) {
        //    await _userService.DeleteUser(id);
        //    return Ok("User deleted successfully.");
        //}

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest) {
            try {
                var userRegistration = _mapper.Map<RegisterRequest, UserRegistration>(registerRequest);
                await _userService.RegisterUser(userRegistration);
            } catch (ValidationException ex) {
                return BadRequest(string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
            }

            return Ok("User registered successfully.");
        }
    }
}
