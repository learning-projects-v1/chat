using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class makemessageIduserIduniqueforseenstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageSeenStatuses_MessageId",
                table: "MessageSeenStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeenStatuses_MessageId_UserId",
                table: "MessageSeenStatuses",
                columns: new[] { "MessageId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageSeenStatuses_MessageId_UserId",
                table: "MessageSeenStatuses");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeenStatuses_MessageId",
                table: "MessageSeenStatuses",
                column: "MessageId");
        }
    }
}
