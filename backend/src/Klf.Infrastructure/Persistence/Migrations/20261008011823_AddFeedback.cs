using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Klf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Definition = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublicCode = table.Column<string>(type: "character(22)", fixedLength: true, maxLength: 22, nullable: false),
                    FormId = table.Column<Guid>(type: "uuid", nullable: true),
                    FormTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Definition = table.Column<string>(type: "jsonb", nullable: false),
                    OpensAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosesAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxResponses = table.Column<int>(type: "integer", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackSessions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackSessions_FeedbackForms_FormId",
                        column: x => x.FormId,
                        principalTable: "FeedbackForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackSessions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Answers = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackResponses_FeedbackSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "FeedbackSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackResponses_SessionId",
                table: "FeedbackResponses",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_ClientId",
                table: "FeedbackSessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_FormId",
                table: "FeedbackSessions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_OpensAt",
                table: "FeedbackSessions",
                column: "OpensAt");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_OwnerId_OpensAt",
                table: "FeedbackSessions",
                columns: new[] { "OwnerId", "OpensAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_PublicCode",
                table: "FeedbackSessions",
                column: "PublicCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSessions_ServiceId",
                table: "FeedbackSessions",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackResponses");

            migrationBuilder.DropTable(
                name: "FeedbackSessions");

            migrationBuilder.DropTable(
                name: "FeedbackForms");
        }
    }
}
