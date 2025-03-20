using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameToCustomZullaRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "CustomZulas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "CustomZulas");
        }
    }
}
