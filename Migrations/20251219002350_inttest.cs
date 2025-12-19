using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReverseMarket.Migrations
{
    /// <inheritdoc />
    public partial class inttest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowEmailNotifications",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowInAppNotifications",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowSMSNotifications",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowWhatsAppNotifications",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-id-12345",
                columns: new[] { "AllowEmailNotifications", "AllowInAppNotifications", "AllowSMSNotifications", "AllowWhatsAppNotifications" },
                values: new object[] { true, true, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowEmailNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowInAppNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowSMSNotifications",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowWhatsAppNotifications",
                table: "Users");
        }
    }
}
