using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdfromCustomZula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomZulas_AspNetUsers_UserId",
                table: "CustomZulas");

            migrationBuilder.DropIndex(
                name: "IX_CustomZulas_UserId",
                table: "CustomZulas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CustomZulas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CustomZulas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CustomZulas_UserId",
                table: "CustomZulas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomZulas_AspNetUsers_UserId",
                table: "CustomZulas",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
