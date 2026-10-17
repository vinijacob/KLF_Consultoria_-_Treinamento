using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertSocialLinksToList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SiteSettings"
                SET "ValueJson" = jsonb_build_object('links', COALESCE((
                    SELECT jsonb_agg(jsonb_build_object('name', network.name, 'url', "ValueJson" ->> network.key) ORDER BY network.position)
                    FROM (VALUES (1, 'instagram', 'Instagram'), (2, 'linkedin', 'LinkedIn'), (3, 'facebook', 'Facebook'), (4, 'youtube', 'YouTube'))
                        AS network(position, key, name)
                    WHERE COALESCE("ValueJson" ->> network.key, '') <> ''), '[]'::jsonb))
                WHERE "Key" = 'social' AND NOT ("ValueJson" ? 'links');
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "SiteSettings"
                SET "ValueJson" = jsonb_build_object(
                    'instagram', (SELECT link ->> 'url' FROM jsonb_array_elements("ValueJson" -> 'links') link WHERE lower(link ->> 'name') = 'instagram' LIMIT 1),
                    'linkedin', (SELECT link ->> 'url' FROM jsonb_array_elements("ValueJson" -> 'links') link WHERE lower(link ->> 'name') = 'linkedin' LIMIT 1),
                    'facebook', (SELECT link ->> 'url' FROM jsonb_array_elements("ValueJson" -> 'links') link WHERE lower(link ->> 'name') = 'facebook' LIMIT 1),
                    'youtube', (SELECT link ->> 'url' FROM jsonb_array_elements("ValueJson" -> 'links') link WHERE lower(link ->> 'name') = 'youtube' LIMIT 1))
                WHERE "Key" = 'social' AND "ValueJson" ? 'links';
                """);

        }
    }
}
