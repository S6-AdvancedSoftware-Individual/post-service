using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostService.Infrastructure.Migrations.PostDbContextSecondaryMigrations
{
    /// <inheritdoc />
    public partial class AddFtsToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
