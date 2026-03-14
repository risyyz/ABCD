using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.DC
{
    /// <inheritdoc />
    public partial class _2026031301 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastPublished",
                table: "Posts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateLastPublished",
                table: "Posts");
        }
    }
}
