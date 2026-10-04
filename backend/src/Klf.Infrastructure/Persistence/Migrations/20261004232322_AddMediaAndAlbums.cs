using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAndAlbums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    AltText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoverId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_MediaAssets_CoverId",
                        column: x => x.CoverId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlbumMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumMedia_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumMedia_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_PhotoId",
                table: "Testimonials",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CoverId",
                table: "Services",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CoverId",
                table: "Posts",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LogoId",
                table: "Clients",
                column: "LogoId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMedia_AlbumId_MediaAssetId",
                table: "AlbumMedia",
                columns: new[] { "AlbumId", "MediaAssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumMedia_MediaAssetId",
                table: "AlbumMedia",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CoverId",
                table: "Albums",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_IsActive_DisplayOrder",
                table: "Albums",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Slug",
                table: "Albums",
                column: "Slug",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_StorageKey",
                table: "MediaAssets",
                column: "StorageKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_MediaAssets_LogoId",
                table: "Clients",
                column: "LogoId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_MediaAssets_CoverId",
                table: "Posts",
                column: "CoverId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_MediaAssets_CoverId",
                table: "Services",
                column: "CoverId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Testimonials_MediaAssets_PhotoId",
                table: "Testimonials",
                column: "PhotoId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_MediaAssets_LogoId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_MediaAssets_CoverId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_MediaAssets_CoverId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Testimonials_MediaAssets_PhotoId",
                table: "Testimonials");

            migrationBuilder.DropTable(
                name: "AlbumMedia");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_PhotoId",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Services_CoverId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CoverId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Clients_LogoId",
                table: "Clients");
        }
    }
}
