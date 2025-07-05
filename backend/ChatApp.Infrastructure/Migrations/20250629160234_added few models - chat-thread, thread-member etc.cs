using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedfewmodelschatthreadthreadmemberetc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "Messages",
                newName: "ChatThreadId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                newName: "IX_Messages_ChatThreadId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatThreads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadName = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsGroup = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatThreads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageSeenStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSeenStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageSeenStatuses_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatThreadParticipents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatThreadParticipents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatThreadParticipents_ChatThreads_ChatThreadId",
                        column: x => x.ChatThreadId,
                        principalTable: "ChatThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatThreadParticipents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreadParticipents_ChatThreadId",
                table: "ChatThreadParticipents",
                column: "ChatThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreadParticipents_UserId",
                table: "ChatThreadParticipents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeenStatuses_MessageId",
                table: "MessageSeenStatuses",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatThreads_ChatThreadId",
                table: "Messages",
                column: "ChatThreadId",
                principalTable: "ChatThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatThreads_ChatThreadId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "ChatThreadParticipents");

            migrationBuilder.DropTable(
                name: "MessageSeenStatuses");

            migrationBuilder.DropTable(
                name: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_Messages_UserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ChatThreadId",
                table: "Messages",
                newName: "ReceiverId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChatThreadId",
                table: "Messages",
                newName: "IX_Messages_ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
