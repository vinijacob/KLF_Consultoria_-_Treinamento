using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTestimonialConsentDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsentDocumentKey",
                table: "Testimonials",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentDocumentName",
                table: "Testimonials",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ConsentDocumentSize",
                table: "Testimonials",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentDocumentUploadedAt",
                table: "Testimonials",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentDocumentKey",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "ConsentDocumentName",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "ConsentDocumentSize",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "ConsentDocumentUploadedAt",
                table: "Testimonials");
        }
    }
}
