using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientsAndTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LogoId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    AuthorRole = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Quote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsentGivenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsentCoversImage = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentRevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsActive_DisplayOrder",
                table: "Clients",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsPublished_DisplayOrder",
                table: "Testimonials",
                columns: new[] { "IsPublished", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Testimonials");
        }
    }
}
