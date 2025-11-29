using Microsoft.EntityFrameworkCore;
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
    }
}
