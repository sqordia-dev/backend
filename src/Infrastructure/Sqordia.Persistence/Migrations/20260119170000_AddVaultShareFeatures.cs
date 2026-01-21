using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVaultShareFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Vault Share columns to BusinessPlanShares table
            migrationBuilder.AddColumn<bool>(
                name: "IsVaultShare",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWatermark",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WatermarkText",
                table: "BusinessPlanShares",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowDownload",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackViews",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailVerification",
                table: "BusinessPlanShares",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "BusinessPlanShares",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxViews",
                table: "BusinessPlanShares",
                type: "integer",
                nullable: true);

            // Add index for vault shares
            migrationBuilder.CreateIndex(
                name: "IX_BusinessPlanShares_IsVaultShare_IsActive",
                table: "BusinessPlanShares",
                columns: new[] { "IsVaultShare", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BusinessPlanShares_IsVaultShare_IsActive",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "IsVaultShare",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "EnableWatermark",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "WatermarkText",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "AllowDownload",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "TrackViews",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "RequireEmailVerification",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "BusinessPlanShares");

            migrationBuilder.DropColumn(
                name: "MaxViews",
                table: "BusinessPlanShares");
        }
    }
}
