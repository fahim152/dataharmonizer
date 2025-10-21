using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmonizer.Migrations
{
    /// <inheritdoc />
    public partial class AddHarmonizeRequestAndRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HarmonizeRequestId",
                table: "HarmonizedRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HarmonizeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HarmonizeRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HarmonizedRecords_HarmonizeRequestId",
                table: "HarmonizedRecords",
                column: "HarmonizeRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_HarmonizedRecords_HarmonizeRequests_HarmonizeRequestId",
                table: "HarmonizedRecords",
                column: "HarmonizeRequestId",
                principalTable: "HarmonizeRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HarmonizedRecords_HarmonizeRequests_HarmonizeRequestId",
                table: "HarmonizedRecords");

            migrationBuilder.DropTable(
                name: "HarmonizeRequests");

            migrationBuilder.DropIndex(
                name: "IX_HarmonizedRecords_HarmonizeRequestId",
                table: "HarmonizedRecords");

            migrationBuilder.DropColumn(
                name: "HarmonizeRequestId",
                table: "HarmonizedRecords");
        }
    }
}
