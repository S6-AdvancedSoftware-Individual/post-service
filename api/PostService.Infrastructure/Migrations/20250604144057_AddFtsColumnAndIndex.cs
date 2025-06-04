using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFtsColumnAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // In the Up method of the migration
            migrationBuilder.Sql(@"
                ALTER TABLE ""Posts""
                ADD COLUMN fts tsvector GENERATED ALWAYS AS (to_tsvector('english', ""Content"")) STORED;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX idx_posts_fts ON ""Posts"" USING gin (fts);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
