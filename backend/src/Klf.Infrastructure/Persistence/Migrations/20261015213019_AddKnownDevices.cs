using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKnownDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnownDevices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnownDevices_LastSeenAt",
                table: "KnownDevices",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnownDevices_UserId_TokenHash",
                table: "KnownDevices",
                columns: new[] { "UserId", "TokenHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownDevices");
        }
    }
}
