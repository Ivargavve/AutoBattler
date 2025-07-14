using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Attackmigreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HealAmount",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocking",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Magic",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PoisonedTurns",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Speed",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealAmount",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "IsBlocking",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Magic",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "PoisonedTurns",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Characters");
        }
    }
}
