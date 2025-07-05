using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modelsreworked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreadParticipents_ChatThreads_ChatThreadId",
                table: "ChatThreadParticipents");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreadParticipents_Users_UserId",
                table: "ChatThreadParticipents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatThreadParticipents",
                table: "ChatThreadParticipents");

            migrationBuilder.RenameTable(
                name: "ChatThreadParticipents",
                newName: "ChatThreadMembers");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreadParticipents_UserId",
                table: "ChatThreadMembers",
                newName: "IX_ChatThreadMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreadParticipents_ChatThreadId",
                table: "ChatThreadMembers",
                newName: "IX_ChatThreadMembers_ChatThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatThreadMembers",
                table: "ChatThreadMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreadMembers_ChatThreads_ChatThreadId",
                table: "ChatThreadMembers",
                column: "ChatThreadId",
                principalTable: "ChatThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreadMembers_Users_UserId",
                table: "ChatThreadMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreadMembers_ChatThreads_ChatThreadId",
                table: "ChatThreadMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreadMembers_Users_UserId",
                table: "ChatThreadMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatThreadMembers",
                table: "ChatThreadMembers");

            migrationBuilder.RenameTable(
                name: "ChatThreadMembers",
                newName: "ChatThreadParticipents");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreadMembers_UserId",
                table: "ChatThreadParticipents",
                newName: "IX_ChatThreadParticipents_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreadMembers_ChatThreadId",
                table: "ChatThreadParticipents",
                newName: "IX_ChatThreadParticipents_ChatThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatThreadParticipents",
                table: "ChatThreadParticipents",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreadParticipents_ChatThreads_ChatThreadId",
                table: "ChatThreadParticipents",
                column: "ChatThreadId",
                principalTable: "ChatThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreadParticipents_Users_UserId",
                table: "ChatThreadParticipents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
