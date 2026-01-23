using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmonizer.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboundApiColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OutboundApiEnabled",
                table: "Workflows",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OutboundApiHeaders",
                table: "Workflows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutboundApiUrl",
                table: "Workflows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SentToOutboundApi",
                table: "HarmonizedRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentToOutboundApiAt",
                table: "HarmonizedRecords",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutboundApiEnabled",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "OutboundApiHeaders",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "OutboundApiUrl",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "SentToOutboundApi",
                table: "HarmonizedRecords");

            migrationBuilder.DropColumn(
                name: "SentToOutboundApiAt",
                table: "HarmonizedRecords");
        }
    }
}
