using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkWise.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Squads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LeaderID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Squads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Squads_AspNetUsers_LeaderID",
                        column: x => x.LeaderID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Desc = table.Column<string>(type: "text", nullable: false),
                    Importancy = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<bool>(type: "boolean", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PerformerID = table.Column<string>(type: "text", nullable: true),
                    SquadId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_AspNetUsers_PerformerID",
                        column: x => x.PerformerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Goals_Squads_SquadId",
                        column: x => x.SquadId,
                        principalTable: "Squads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSquad",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SquadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSquad", x => new { x.UserId, x.SquadId });
                    table.ForeignKey(
                        name: "FK_UserSquad_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSquad_Squads_SquadId",
                        column: x => x.SquadId,
                        principalTable: "Squads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedById = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalAttachments_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalAttachments_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalFeedback_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalFeedback_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalAttachments_GoalId",
                table: "GoalAttachments",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalAttachments_UploadedById",
                table: "GoalAttachments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_GoalFeedback_AuthorId",
                table: "GoalFeedback",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalFeedback_GoalId",
                table: "GoalFeedback",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PerformerID",
                table: "Goals",
                column: "PerformerID");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_SquadId",
                table: "Goals",
                column: "SquadId");

            migrationBuilder.CreateIndex(
                name: "IX_Squads_LeaderID",
                table: "Squads",
                column: "LeaderID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSquad_SquadId",
                table: "UserSquad",
                column: "SquadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalAttachments");

            migrationBuilder.DropTable(
                name: "GoalFeedback");

            migrationBuilder.DropTable(
                name: "UserSquad");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Squads");
        }
    }
}
