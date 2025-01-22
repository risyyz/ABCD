using ABCD.Lib;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class AuthContext : IdentityDbContext<ApplicationUser> {
    private readonly IOptions<Settings> _settings;

    public AuthContext(DbContextOptions<AuthContext> options, IOptions<Settings> settings) : base(options) {
        _settings = settings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            optionsBuilder.UseSqlServer(_settings.Value.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables to use a different schema
        modelBuilder.Entity<ApplicationUser>(entity => entity.ToTable(name: "Users", schema: "auth"));
        modelBuilder.Entity<IdentityRole>(entity => entity.ToTable(name: "Roles", schema: "auth"));
        modelBuilder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles", schema: "auth"));
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims", schema: "auth"));
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins", schema: "auth"));
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims", schema: "auth"));
        modelBuilder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens", schema: "auth"));
    }
}

public class ApplicationUser : IdentityUser { }
