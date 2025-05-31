using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedvariablelengthuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7017));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7030));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7036));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7042));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7048));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7056));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7062));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7067));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7073));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "HashedPassword", "IsOnline", "RefreshToken", "UserName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000010"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7080), "User-10@gmail.com", "User 10", "123456", false, null, "user10" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7087), "User-11@gmail.com", "User 11", "123456", false, null, "user11" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7104), "User-12@gmail.com", "User 12", "123456", false, null, "user12" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7110), "User-13@gmail.com", "User 13", "123456", false, null, "user13" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7116), "User-14@gmail.com", "User 14", "123456", false, null, "user14" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), new DateTime(2025, 5, 31, 17, 43, 12, 282, DateTimeKind.Utc).AddTicks(7121), "User-15@gmail.com", "User 15", "123456", false, null, "user15" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3734));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3759));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3766));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3772));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3785));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3793));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3809));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3816));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"),
                column: "CreatedAt",
                value: new DateTime(2025, 5, 31, 17, 35, 30, 899, DateTimeKind.Utc).AddTicks(3838));
        }
    }
}
