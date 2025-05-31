using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seed10user2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "HashedPassword", "IsOnline", "RefreshToken", "UserName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3734), "User-1@gmail.com", "User 1", "123456", false, null, "user1" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3759), "User-2@gmail.com", "User 2", "123456", false, null, "user2" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3766), "User-3@gmail.com", "User 3", "123456", false, null, "user3" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3772), "User-4@gmail.com", "User 4", "123456", false, null, "user4" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3785), "User-5@gmail.com", "User 5", "123456", false, null, "user5" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3793), "User-6@gmail.com", "User 6", "123456", false, null, "user6" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3809), "User-7@gmail.com", "User 7", "123456", false, null, "user7" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3816), "User-8@gmail.com", "User 8", "123456", false, null, "user8" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3838), "User-9@gmail.com", "User 9", "123456", false, null, "user9" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
