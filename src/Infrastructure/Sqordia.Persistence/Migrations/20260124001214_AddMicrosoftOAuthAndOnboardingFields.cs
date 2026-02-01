using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrosoftOAuthAndOnboardingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MicrosoftId",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingData",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingStep",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MicrosoftId",
                table: "Users",
                column: "MicrosoftId",
                unique: true,
                filter: "\"MicrosoftId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_MicrosoftId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MicrosoftId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OnboardingData",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OnboardingStep",
                table: "Users");
        }
    }
}
