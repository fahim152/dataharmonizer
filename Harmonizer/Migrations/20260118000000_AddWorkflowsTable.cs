using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmonizer.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkflowId",
                table: "HarmonizeRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ContextPrompt = table.Column<string>(type: "TEXT", nullable: false),
                    DesiredOutput = table.Column<string>(type: "TEXT", nullable: false),
                    DesiredOutputType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HarmonizeRequests_WorkflowId",
                table: "HarmonizeRequests",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ApiKey",
                table: "Workflows",
                column: "ApiKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_HarmonizeRequests_Workflows_WorkflowId",
                table: "HarmonizeRequests",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HarmonizeRequests_Workflows_WorkflowId",
                table: "HarmonizeRequests");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropIndex(
                name: "IX_HarmonizeRequests_WorkflowId",
                table: "HarmonizeRequests");

            migrationBuilder.DropColumn(
                name: "WorkflowId",
                table: "HarmonizeRequests");
        }
    }
}
