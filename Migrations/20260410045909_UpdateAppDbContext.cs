using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3demo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_AppUserId",
                table: "SavedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SavedDocuments_AppUserId",
                table: "SavedDocuments");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "SavedDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "SavedDocuments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedDocuments_AppUserId",
                table: "SavedDocuments",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_AppUserId",
                table: "SavedDocuments",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
