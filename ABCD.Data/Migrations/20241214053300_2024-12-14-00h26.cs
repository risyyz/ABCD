using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCD.Data.Migrations
{
    /// <inheritdoc />
    public partial class _2024121400h26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogId);
                });

            migrationBuilder.CreateTable(
                name: "BlogDomains",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogDomains", x => new { x.BlogId, x.Domain });
                    table.ForeignKey(
                        name: "FK_BlogDomains_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogDomains");

            migrationBuilder.DropTable(
                name: "Blogs");
        }
    }
}
