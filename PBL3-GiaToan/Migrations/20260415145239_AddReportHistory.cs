using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3demo.Migrations
{
    /// <inheritdoc />
    public partial class AddReportHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionTaken",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionTaken",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "Reports");
        }
    }
}
