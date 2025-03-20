using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyECommerce.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullableInCustomZula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomZulas_AspNetUsers_UserId",
                table: "CustomZulas");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CustomZulas",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomZulas_AspNetUsers_UserId",
                table: "CustomZulas",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomZulas_AspNetUsers_UserId",
                table: "CustomZulas");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CustomZulas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
