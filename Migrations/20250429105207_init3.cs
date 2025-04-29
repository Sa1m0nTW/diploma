using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkWise.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalFeedback_AspNetUsers_AuthorId",
                table: "GoalFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalFeedback_Goals_GoalId",
                table: "GoalFeedback");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSquad_AspNetUsers_UserId",
                table: "UserSquad");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSquad_Squads_SquadId",
                table: "UserSquad");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSquad",
                table: "UserSquad");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalFeedback",
                table: "GoalFeedback");

            migrationBuilder.RenameTable(
                name: "UserSquad",
                newName: "UserSquads");

            migrationBuilder.RenameTable(
                name: "GoalFeedback",
                newName: "GoalFeedbacks");

            migrationBuilder.RenameIndex(
                name: "IX_UserSquad_SquadId",
                table: "UserSquads",
                newName: "IX_UserSquads_SquadId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalFeedback_GoalId",
                table: "GoalFeedbacks",
                newName: "IX_GoalFeedbacks_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalFeedback_AuthorId",
                table: "GoalFeedbacks",
                newName: "IX_GoalFeedbacks_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSquads",
                table: "UserSquads",
                columns: new[] { "UserId", "SquadId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalFeedbacks",
                table: "GoalFeedbacks",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<string>(type: "text", nullable: false),
                    InviterUserId = table.Column<string>(type: "text", nullable: false),
                    SquadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_AspNetUsers_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invitations_AspNetUsers_InviterUserId",
                        column: x => x.InviterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invitations_Squads_SquadId",
                        column: x => x.SquadId,
                        principalTable: "Squads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_InvitedUserId",
                table: "Invitations",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_InviterUserId",
                table: "Invitations",
                column: "InviterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_SquadId",
                table: "Invitations",
                column: "SquadId");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalFeedbacks_AspNetUsers_AuthorId",
                table: "GoalFeedbacks",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalFeedbacks_Goals_GoalId",
                table: "GoalFeedbacks",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSquads_AspNetUsers_UserId",
                table: "UserSquads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSquads_Squads_SquadId",
                table: "UserSquads",
                column: "SquadId",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalFeedbacks_AspNetUsers_AuthorId",
                table: "GoalFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalFeedbacks_Goals_GoalId",
                table: "GoalFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSquads_AspNetUsers_UserId",
                table: "UserSquads");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSquads_Squads_SquadId",
                table: "UserSquads");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSquads",
                table: "UserSquads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GoalFeedbacks",
                table: "GoalFeedbacks");

            migrationBuilder.RenameTable(
                name: "UserSquads",
                newName: "UserSquad");

            migrationBuilder.RenameTable(
                name: "GoalFeedbacks",
                newName: "GoalFeedback");

            migrationBuilder.RenameIndex(
                name: "IX_UserSquads_SquadId",
                table: "UserSquad",
                newName: "IX_UserSquad_SquadId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalFeedbacks_GoalId",
                table: "GoalFeedback",
                newName: "IX_GoalFeedback_GoalId");

            migrationBuilder.RenameIndex(
                name: "IX_GoalFeedbacks_AuthorId",
                table: "GoalFeedback",
                newName: "IX_GoalFeedback_AuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSquad",
                table: "UserSquad",
                columns: new[] { "UserId", "SquadId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_GoalFeedback",
                table: "GoalFeedback",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalFeedback_AspNetUsers_AuthorId",
                table: "GoalFeedback",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalFeedback_Goals_GoalId",
                table: "GoalFeedback",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSquad_AspNetUsers_UserId",
                table: "UserSquad",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSquad_Squads_SquadId",
                table: "UserSquad",
                column: "SquadId",
                principalTable: "Squads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
