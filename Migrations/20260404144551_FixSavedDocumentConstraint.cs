using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3demo.Migrations
{
    /// <inheritdoc />
    public partial class FixSavedDocumentConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_UserId",
                table: "SavedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedDocuments_Documents_DocumentId",
                table: "SavedDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_UserId",
                table: "SavedDocuments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDocuments_Documents_DocumentId",
                table: "SavedDocuments",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_UserId",
                table: "SavedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedDocuments_Documents_DocumentId",
                table: "SavedDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDocuments_AspNetUsers_UserId",
                table: "SavedDocuments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedDocuments_Documents_DocumentId",
                table: "SavedDocuments",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
