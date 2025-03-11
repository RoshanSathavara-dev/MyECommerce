using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class Removetitlefromgalleryimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "GalleryImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "GalleryImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
