using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.AC
{
    /// <inheritdoc />
    public partial class _2026041501 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Moves Identity tables from auth schema to dbo.
            // No-op on fresh databases where migration 1 already creates in dbo.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'auth')
                BEGIN
                    IF OBJECT_ID('auth.UserTokens') IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[UserTokens];
                    IF OBJECT_ID('auth.UserLogins') IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[UserLogins];
                    IF OBJECT_ID('auth.UserRoles')  IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[UserRoles];
                    IF OBJECT_ID('auth.UserClaims') IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[UserClaims];
                    IF OBJECT_ID('auth.RoleClaims') IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[RoleClaims];
                    IF OBJECT_ID('auth.Users')      IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[Users];
                    IF OBJECT_ID('auth.Roles')      IS NOT NULL ALTER SCHEMA dbo TRANSFER auth.[Roles];
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: dbo is the target schema, reverting to auth is not supported
        }
    }
}
