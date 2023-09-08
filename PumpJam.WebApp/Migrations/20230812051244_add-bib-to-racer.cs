using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PumpJam.Migrations
{
    /// <inheritdoc />
    public partial class addbibtoracer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Racers_Categories_CategoryId",
                table: "Racers");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Racers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Bib",
                table: "Racers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Racers_Categories_CategoryId",
                table: "Racers",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Racers_Categories_CategoryId",
                table: "Racers");

            migrationBuilder.DropColumn(
                name: "Bib",
                table: "Racers");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Racers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Racers_Categories_CategoryId",
                table: "Racers",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
