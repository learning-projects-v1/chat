using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class builderrorfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReactionToMessageId",
                table: "Reactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Reactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Reactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_ReactionToMessageId",
                table: "Reactions",
                column: "ReactionToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_UserId",
                table: "Reactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_UserId1",
                table: "Reactions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId",
                table: "Reactions",
                column: "ReactionToMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Users_UserId",
                table: "Reactions",
                column: "UserId",
                principalTable: "Users",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Messages_ReactionToMessageId",
                table: "Reactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Users_UserId",
                table: "Reactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Users_UserId1",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_ReactionToMessageId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_UserId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_UserId1",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "ReactionToMessageId",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Reactions");
        }
    }
}
