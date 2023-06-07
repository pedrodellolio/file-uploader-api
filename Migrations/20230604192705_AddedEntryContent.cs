using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace file_uploader_api.Migrations
{
    /// <inheritdoc />
    public partial class AddedEntryContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SizeInBytes",
                table: "Entries",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Entries",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Entries");

            migrationBuilder.AlterColumn<byte>(
                name: "SizeInBytes",
                table: "Entries",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
