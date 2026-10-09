using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstructorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedbackSessions_OwnerId_OpensAt",
                table: "FeedbackSessions");

            migrationBuilder.Sql("""
                DELETE FROM "AspNetUserRoles" WHERE "RoleId" IN (SELECT "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'INSTRUCTOR');
                DELETE FROM "AspNetRoleClaims" WHERE "RoleId" IN (SELECT "Id" FROM "AspNetRoles" WHERE "NormalizedName" = 'INSTRUCTOR');
                DELETE FROM "AspNetRoles" WHERE "NormalizedName" = 'INSTRUCTOR';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_OwnerId_OpensAt",
                table: "FeedbackSessions",
                columns: new[] { "OwnerId", "OpensAt" });
        }
    }
}
