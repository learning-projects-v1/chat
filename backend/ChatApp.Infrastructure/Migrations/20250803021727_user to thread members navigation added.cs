using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class usertothreadmembersnavigationadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreadMembers_Users_UserId1",
                table: "ChatThreadMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreadMembers_ChatThreadId",
                table: "ChatThreadMembers");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreadMembers_UserId1",
                table: "ChatThreadMembers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ChatThreadMembers");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreadMembers_ChatThreadId_UserId",
                table: "ChatThreadMembers",
                columns: new[] { "ChatThreadId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatThreadMembers_ChatThreadId_UserId",
                table: "ChatThreadMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "ChatThreadMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreadMembers_ChatThreadId",
                table: "ChatThreadMembers",
                column: "ChatThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreadMembers_UserId1",
                table: "ChatThreadMembers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreadMembers_Users_UserId1",
                table: "ChatThreadMembers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
