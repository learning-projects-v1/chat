using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class efseenstatusmodelchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Messages");

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId1",
                table: "MessageSeenStatuses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeenStatuses_MessageId1",
                table: "MessageSeenStatuses",
                column: "MessageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageSeenStatuses_Messages_MessageId1",
                table: "MessageSeenStatuses",
                column: "MessageId1",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageSeenStatuses_Messages_MessageId1",
                table: "MessageSeenStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MessageSeenStatuses_MessageId1",
                table: "MessageSeenStatuses");

            migrationBuilder.DropColumn(
                name: "MessageId1",
                table: "MessageSeenStatuses");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
