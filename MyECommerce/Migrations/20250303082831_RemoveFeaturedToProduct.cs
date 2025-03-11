using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFeaturedToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
