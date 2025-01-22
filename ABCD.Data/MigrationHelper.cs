using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ABCD.Data
{
    public static class MigrationHelper
    {
        public static void ApplyMigrations(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            dataContext.Database.Migrate();
            var authContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
            authContext.Database.Migrate();
        }
    }
}
