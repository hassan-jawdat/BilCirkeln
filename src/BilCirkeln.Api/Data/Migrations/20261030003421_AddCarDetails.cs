using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilCirkeln.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCarDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasExtraTires",
                table: "CarSubmissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasServiceBook",
                table: "CarSubmissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectionStatus",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InteriorColor",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InteriorMaterial",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinanced",
                table: "CarSubmissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "CarSubmissions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyCount",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerType",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceHistory",
                table: "CarSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasExtraTires",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "HasServiceBook",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "InspectionStatus",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "InteriorColor",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "InteriorMaterial",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "IsFinanced",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "KeyCount",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "SellerType",
                table: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "ServiceHistory",
                table: "CarSubmissions");
        }
    }
}
