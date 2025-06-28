using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedreactionsmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000015"));

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reactions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_MessageId",
                table: "Reactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_MessageId_UserId",
                table: "Reactions",
                columns: new[] { "MessageId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "HashedPassword", "IsOnline", "RefreshToken", "UserName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7017), "User-1@gmail.com", "User 1", "123456", false, null, "user1" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7030), "User-2@gmail.com", "User 2", "123456", false, null, "user2" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7036), "User-3@gmail.com", "User 3", "123456", false, null, "user3" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7042), "User-4@gmail.com", "User 4", "123456", false, null, "user4" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7048), "User-5@gmail.com", "User 5", "123456", false, null, "user5" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7056), "User-6@gmail.com", "User 6", "123456", false, null, "user6" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7062), "User-7@gmail.com", "User 7", "123456", false, null, "user7" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7067), "User-8@gmail.com", "User 8", "123456", false, null, "user8" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7073), "User-9@gmail.com", "User 9", "123456", false, null, "user9" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7080), "User-10@gmail.com", "User 10", "123456", false, null, "user10" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7087), "User-11@gmail.com", "User 11", "123456", false, null, "user11" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7104), "User-12@gmail.com", "User 12", "123456", false, null, "user12" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7110), "User-13@gmail.com", "User 13", "123456", false, null, "user13" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7116), "User-14@gmail.com", "User 14", "123456", false, null, "user14" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7121), "User-15@gmail.com", "User 15", "123456", false, null, "user15" }
                });
        }
    }
}
