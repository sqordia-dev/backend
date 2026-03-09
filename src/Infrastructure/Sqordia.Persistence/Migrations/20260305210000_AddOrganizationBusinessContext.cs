using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sqordia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationBusinessContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamSize",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundingStatus",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetMarket",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessStage",
                table: "Organizations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoalsJson",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Organizations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileCompletenessScore",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Industry", table: "Organizations");
            migrationBuilder.DropColumn(name: "Sector", table: "Organizations");
            migrationBuilder.DropColumn(name: "TeamSize", table: "Organizations");
            migrationBuilder.DropColumn(name: "FundingStatus", table: "Organizations");
            migrationBuilder.DropColumn(name: "TargetMarket", table: "Organizations");
            migrationBuilder.DropColumn(name: "BusinessStage", table: "Organizations");
            migrationBuilder.DropColumn(name: "GoalsJson", table: "Organizations");
            migrationBuilder.DropColumn(name: "City", table: "Organizations");
            migrationBuilder.DropColumn(name: "Province", table: "Organizations");
            migrationBuilder.DropColumn(name: "Country", table: "Organizations");
            migrationBuilder.DropColumn(name: "ProfileCompletenessScore", table: "Organizations");
        }
    }
}
