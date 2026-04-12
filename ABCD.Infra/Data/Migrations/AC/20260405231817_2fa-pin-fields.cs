using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.AC
{
    /// <inheritdoc />
    public partial class _2fapinfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwoFactorPin",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TwoFactorPinExpiry",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorPin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorPinExpiry",
                table: "Users");
        }
    }
}
