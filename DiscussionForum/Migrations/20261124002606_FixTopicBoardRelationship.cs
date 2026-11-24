using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Migrations
{
    /// <inheritdoc />
    public partial class FixTopicBoardRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_DiscussionBoards_DiscussionBoardBoardId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_DiscussionBoardBoardId",
                table: "Topics");

            migrationBuilder.DropColumn(
                name: "DiscussionBoardBoardId",
                table: "Topics");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_BoardId",
                table: "Topics",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_DiscussionBoards_BoardId",
                table: "Topics",
                column: "BoardId",
                principalTable: "DiscussionBoards",
                principalColumn: "BoardId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Topics_DiscussionBoards_BoardId",
                table: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Topics_BoardId",
                table: "Topics");

            migrationBuilder.AddColumn<int>(
                name: "DiscussionBoardBoardId",
                table: "Topics",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Topics_DiscussionBoardBoardId",
                table: "Topics",
                column: "DiscussionBoardBoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Topics_DiscussionBoards_DiscussionBoardBoardId",
                table: "Topics",
                column: "DiscussionBoardBoardId",
                principalTable: "DiscussionBoards",
                principalColumn: "BoardId");
        }
    }
}
