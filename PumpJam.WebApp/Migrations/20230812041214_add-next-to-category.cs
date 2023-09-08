using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PumpJam.Migrations
{
    /// <inheritdoc />
    public partial class addnexttocategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bib",
                table: "Categories");

            migrationBuilder.AddColumn<bool>(
                name: "Next",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Next",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "Bib",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
