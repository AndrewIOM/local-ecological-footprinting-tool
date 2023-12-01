using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecoset.WebUI.Migrations
{
    public partial class Datapackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCostIncludingVat",
                table: "ShoppingBaskets",
                type: "decimal(6,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PerUnitCost",
                table: "ShoppingBaskets",
                type: "decimal(6,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "Purchases",
                type: "decimal(6,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Purchases",
                type: "decimal(6,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "PriceThresholds",
                type: "decimal(6,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "Hidden",
                table: "Jobs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DataPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    JobProcessorReference = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    LatitudeSouth = table.Column<double>(nullable: false),
                    LatitudeNorth = table.Column<double>(nullable: false),
                    LongitudeEast = table.Column<double>(nullable: false),
                    LongitudeWest = table.Column<double>(nullable: false),
                    TimeRequested = table.Column<DateTime>(nullable: false),
                    TimeCompleted = table.Column<DateTime>(nullable: true),
                    RequestComponents = table.Column<string>(nullable: true),
                    DataRequestedTime = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: true),
                    Month = table.Column<int>(nullable: true),
                    Day = table.Column<int>(nullable: true),
                    CreatedById = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPackages_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    Expires = table.Column<DateTime>(nullable: true),
                    Revoked = table.Column<bool>(nullable: false),
                    RateLimit = table.Column<int>(nullable: true),
                    AnalysisCap = table.Column<int>(nullable: true),
                    PrimaryContactId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_PrimaryContactId",
                        column: x => x.PrimaryContactId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupSubscriptions",
                columns: table => new
                {
                    GroupSubscriptionId = table.Column<Guid>(nullable: false),
                    GroupName = table.Column<string>(nullable: true),
                    EmailWildcard = table.Column<string>(nullable: true),
                    SubscriptionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSubscriptions", x => x.GroupSubscriptionId);
                    table.ForeignKey(
                        name: "FK_GroupSubscriptions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataPackages_CreatedById",
                table: "DataPackages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSubscriptions_SubscriptionId",
                table: "GroupSubscriptions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PrimaryContactId",
                table: "Subscriptions",
                column: "PrimaryContactId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataPackages");

            migrationBuilder.DropTable(
                name: "GroupSubscriptions");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Hidden",
                table: "Jobs");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalCostIncludingVat",
                table: "ShoppingBaskets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PerUnitCost",
                table: "ShoppingBaskets",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Total",
                table: "Purchases",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "PriceThresholds",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)");
        }
    }
}
