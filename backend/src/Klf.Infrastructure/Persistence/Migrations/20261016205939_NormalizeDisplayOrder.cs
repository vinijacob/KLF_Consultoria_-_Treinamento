using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Services" AS item
                SET "DisplayOrder" = ranked.position
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "DisplayOrder", "Title") - 1 AS position
                    FROM "Services"
                    WHERE "DeletedAt" IS NULL) AS ranked
                WHERE item."Id" = ranked."Id" AND item."DisplayOrder" <> ranked.position;
                """);

            migrationBuilder.Sql("""
                UPDATE "Clients" AS item
                SET "DisplayOrder" = ranked.position
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "DisplayOrder", "Name") - 1 AS position
                    FROM "Clients"
                    WHERE "DeletedAt" IS NULL) AS ranked
                WHERE item."Id" = ranked."Id" AND item."DisplayOrder" <> ranked.position;
                """);

            migrationBuilder.Sql("""
                UPDATE "Albums" AS item
                SET "DisplayOrder" = ranked.position
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "DisplayOrder", "Title") - 1 AS position
                    FROM "Albums"
                    WHERE "DeletedAt" IS NULL) AS ranked
                WHERE item."Id" = ranked."Id" AND item."DisplayOrder" <> ranked.position;
                """);

            migrationBuilder.Sql("""
                UPDATE "Testimonials" AS item
                SET "DisplayOrder" = ranked.position
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "DisplayOrder", "AuthorName") - 1 AS position
                    FROM "Testimonials"
                    WHERE "DeletedAt" IS NULL) AS ranked
                WHERE item."Id" = ranked."Id" AND item."DisplayOrder" <> ranked.position;
                """);

            migrationBuilder.Sql("""
                UPDATE "CareerEntries" AS item
                SET "DisplayOrder" = ranked.position
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "EntryType" ORDER BY "DisplayOrder", "Title") - 1 AS position
                    FROM "CareerEntries"
                    WHERE "DeletedAt" IS NULL) AS ranked
                WHERE item."Id" = ranked."Id" AND item."DisplayOrder" <> ranked.position;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
