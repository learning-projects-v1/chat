using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class builderrorfix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId1",
                table: "Reactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Users_UserId1",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_ReactionToMessageId1",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_UserId1",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "ReactionToMessageId1",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Reactions");

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "Reactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_MessageId",
                table: "Reactions",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Messages_MessageId",
                table: "Reactions",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Messages_MessageId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_MessageId",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Reactions");

            migrationBuilder.AddColumn<Guid>(
                name: "ReactionToMessageId1",
                table: "Reactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Reactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactionToMessageId1",
                table: "Reactions",
                column: "ReactionToMessageId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_UserId1",
                table: "Reactions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId1",
                table: "Reactions",
                column: "ReactionToMessageId1",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Users_UserId1",
                table: "Reactions",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
