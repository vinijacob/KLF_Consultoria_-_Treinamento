using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedQuoteAndPortraitCarousel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SiteSettings"
                SET "ValueJson" = ("ValueJson" - 'portraitId')
                    || CASE WHEN jsonb_typeof("ValueJson" -> 'portraitId') = 'string'
                            THEN jsonb_build_object('portraitIds', jsonb_build_array("ValueJson" -> 'portraitId'))
                            ELSE '{}'::jsonb END
                WHERE "Key" = 'about' AND "ValueJson" ? 'portraitId';
                """);

            migrationBuilder.CreateTable(
                name: "FeaturedQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Author = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Source = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedQuotes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SiteSettings"
                SET "ValueJson" = ("ValueJson" - 'portraitIds')
                    || CASE WHEN jsonb_array_length(COALESCE("ValueJson" -> 'portraitIds', '[]'::jsonb)) > 0
                            THEN jsonb_build_object('portraitId', "ValueJson" -> 'portraitIds' -> 0)
                            ELSE '{}'::jsonb END
                WHERE "Key" = 'about' AND "ValueJson" ? 'portraitIds';
                """);

            migrationBuilder.DropTable(
                name: "FeaturedQuotes");
        }
    }
}
