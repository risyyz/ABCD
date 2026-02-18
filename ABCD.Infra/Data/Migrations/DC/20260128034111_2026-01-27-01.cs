using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Infra.Data.Migrations.DC
{
    /// <inheritdoc />
    public partial class _2026012701 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Fragments",
                table: "Fragments");

            migrationBuilder.AddColumn<int>(
                name: "FragmentId",
                table: "Fragments",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fragments",
                table: "Fragments",
                column: "FragmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Fragments_PostId_Position",
                table: "Fragments",
                columns: new[] { "PostId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Fragments",
                table: "Fragments");

            migrationBuilder.DropIndex(
                name: "IX_Fragments_PostId_Position",
                table: "Fragments");

            migrationBuilder.DropColumn(
                name: "FragmentId",
                table: "Fragments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fragments",
                table: "Fragments",
                columns: new[] { "PostId", "Position" });
        }
    }
}
