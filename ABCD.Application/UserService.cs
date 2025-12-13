using ABCD.Infra.Data;

using FluentValidation;

using Microsoft.AspNetCore.Identity;

namespace ABCD.Application {

    public interface IUserService {
        Task<IEnumerable<ApplicationUser>> GetUsers();
        //Task<User> GetUser(string id);
        //Task UpdateUser(string id, UpdateUser user);
        //Task DeleteUser(string id);
        Task RegisterUser(UserRegistration userRegistration);
    }

    public class UserService : IUserService {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<UserRegistration> _userRegistrationValidator;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IValidator<UserRegistration> userRegistrationValidator
        ) {
            _userManager = userManager;
            _userRegistrationValidator = userRegistrationValidator;
        }

        public async Task RegisterUser(UserRegistration userRegistration) {
            _userRegistrationValidator.ValidateAndThrow(userRegistration);
            var user = new ApplicationUser { UserName = userRegistration.Email, Email = userRegistration.Email, RefreshToken = "abcd" };
            await _userManager.CreateAsync(user, userRegistration.Password);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsers() {
            return new List<ApplicationUser> {
                new ApplicationUser { UserName = "user1", Email = "user1@email.com" },
                new ApplicationUser { UserName = "user2", Email = "user2@email.com" }
            };
        }
    }
}
