using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace file_uploader_api.Migrations
{
    /// <inheritdoc />
    public partial class AddedParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Entries_EntryId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_EntryId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Parent",
                table: "Entries");

            migrationBuilder.RenameColumn(
                name: "EntryId",
                table: "Entries",
                newName: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "Entries",
                newName: "EntryId");

            migrationBuilder.AddColumn<string>(
                name: "Parent",
                table: "Entries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_EntryId",
                table: "Entries",
                column: "EntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Entries_EntryId",
                table: "Entries",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id");
        }
    }
}
