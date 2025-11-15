using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Data.Migrations.Data
{
    /// <inheritdoc />
    public partial class _07 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DomainName",
                table: "BlogDomains",
                newName: "Domain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "BlogDomains",
                newName: "DomainName");
        }
    }
}
