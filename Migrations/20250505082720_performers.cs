using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkWise.Migrations
{
    /// <inheritdoc />
    public partial class performers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_AspNetUsers_PerformerID",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_PerformerID",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "PerformerID",
                table: "Goals");

            migrationBuilder.CreateTable(
                name: "GoalPerformers",
                columns: table => new
                {
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalPerformers", x => new { x.GoalId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GoalPerformers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalPerformers_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalPerformers_UserId",
                table: "GoalPerformers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalPerformers");

            migrationBuilder.AddColumn<string>(
                name: "PerformerID",
                table: "Goals",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PerformerID",
                table: "Goals",
                column: "PerformerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_AspNetUsers_PerformerID",
                table: "Goals",
                column: "PerformerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
