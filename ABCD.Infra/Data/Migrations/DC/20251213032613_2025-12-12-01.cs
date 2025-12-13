using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.DC
{
    /// <inheritdoc />
    public partial class _2025121201 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Posts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "Posts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ParentPostId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Posts",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Synopsis",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Posts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Excluded",
                table: "Fragments",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ParentPostId",
                table: "Posts",
                column: "ParentPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Slug",
                table: "Posts",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_ParentPostId",
                table: "Posts",
                column: "ParentPostId",
                principalTable: "Posts",
                principalColumn: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_ParentPostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ParentPostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Slug",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ParentPostId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Synopsis",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Excluded",
                table: "Fragments");
        }
    }
}
