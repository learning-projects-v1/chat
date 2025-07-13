using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class builderrorfix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Messages_MessageId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_MessageId_UserId",
                table: "Reactions");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Reactions",
                newName: "ReactionToMessageId1");

            migrationBuilder.RenameIndex(
                name: "IX_Reactions_MessageId",
                table: "Reactions",
                newName: "IX_Reactions_ReactionToMessageId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactionToMessageId_UserId",
                table: "Reactions",
                columns: new[] { "ReactionToMessageId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId1",
                table: "Reactions",
                column: "ReactionToMessageId1",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId1",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_ReactionToMessageId_UserId",
                table: "Reactions");

            migrationBuilder.RenameColumn(
                name: "ReactionToMessageId1",
                table: "Reactions",
                newName: "MessageId");

            migrationBuilder.RenameIndex(
                name: "IX_Reactions_ReactionToMessageId1",
                table: "Reactions",
                newName: "IX_Reactions_MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_MessageId_UserId",
                table: "Reactions",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Messages_MessageId",
                table: "Reactions",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
