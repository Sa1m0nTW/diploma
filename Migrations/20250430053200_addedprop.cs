using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkWise.Migrations
{
    /// <inheritdoc />
    public partial class addedprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLeaderComment",
                table: "GoalFeedbacks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLeaderComment",
                table: "GoalFeedbacks");
        }
    }
}
