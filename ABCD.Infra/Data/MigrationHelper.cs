using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABCD.Infra.Data {
    public static class MigrationHelper {
        public static void ApplyMigrations(IServiceProvider serviceProvider) {
            using var scope = serviceProvider.CreateScope();
            var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            dataContext.Database.Migrate();
            var authContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
            authContext.Database.Migrate();
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider) {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var email = config["Admin:Email"] ?? "admin@admin.com";

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
                return;

            var password = $"{DateTime.UtcNow:yyyy-MM-dd}#aBc123";
            var user = new ApplicationUser {
                UserName = email,
                Email = email,
                RefreshToken = string.Empty
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
