using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Posts",
                schema: "pg_catalog",
                newName: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pg_catalog");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Posts",
                newSchema: "pg_catalog");
        }
    }
}
