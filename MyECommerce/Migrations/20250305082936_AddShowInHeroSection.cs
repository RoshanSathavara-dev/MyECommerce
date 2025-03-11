using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddShowInHeroSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowInHeroSection",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowInHeroSection",
                table: "Products");
        }
    }
}
