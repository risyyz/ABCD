using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.DC
{
    /// <inheritdoc />
    public partial class _2026021301 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Posts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Fragments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Fragments");
        }
    }
}
