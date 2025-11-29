using ABCD.Infra.Data;

namespace ABCD.Application {

    public interface IUserService {
        Task<IEnumerable<ApplicationUser>> GetUsers();
        //Task<User> GetUser(string id);
        //Task UpdateUser(string id, UpdateUser user);
        //Task DeleteUser(string id);
        //Task RegisterUser(UserRegistration userRegistration);
    }

    public class UserService : IUserService {
        //public async Task RegisterUser(UserRegistration userRegistration) {
        //    _userRegistrationValidator.ValidateAndThrow(userRegistration);
        //    var user = new ApplicationUser { UserName = userRegistration.Email, Email = userRegistration.Email };
        //    await _userManager.CreateAsync(user, userRegistration.Password);
        //}
        public async Task<IEnumerable<ApplicationUser>> GetUsers() {
            return new List<ApplicationUser> {
                new ApplicationUser { UserName = "user1", Email = "user1@email.com" },
                new ApplicationUser { UserName = "user2", Email = "user2@email.com" }
            };
        }
    }
}
