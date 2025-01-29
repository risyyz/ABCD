using Microsoft.AspNetCore.Identity;

namespace ABCD.Services {
    public interface IUserService {
        Task<IdentityResult> RegisterUser(string email, string password);
    }

    public class UserService : IUserService {
        private readonly UserManager<ApplicationUser> userManager;

        public UserService(UserManager<ApplicationUser> userManager) {
            this.userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUser(string email, string password) {
            var user = new ApplicationUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(user, password);
            return result;
        }
    }
}
