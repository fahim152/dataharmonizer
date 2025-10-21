using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmonizer.Migrations
{
    /// <inheritdoc />
    public partial class AddHarmonizedJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HarmonizedJson",
                table: "HarmonizedRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HarmonizedJson",
                table: "HarmonizedRecords");
        }
    }
}
