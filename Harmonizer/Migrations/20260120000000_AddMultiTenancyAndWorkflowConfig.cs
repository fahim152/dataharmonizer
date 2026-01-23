using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmonizer.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyAndWorkflowConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Tenants table
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            // Create Namespaces table
            migrationBuilder.CreateTable(
                name: "Namespaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Namespaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Namespaces_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Namespaces_TenantId_Slug",
                table: "Namespaces",
                columns: new[] { "TenantId", "Slug" },
                unique: true);

            // Create Teams table
            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    NamespaceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Namespaces_NamespaceId",
                        column: x => x.NamespaceId,
                        principalTable: "Namespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_NamespaceId_Slug",
                table: "Teams",
                columns: new[] { "NamespaceId", "Slug" },
                unique: true);

            // Add TeamId and EntityName to Workflows
            migrationBuilder.AddColumn<int>(
                name: "TeamId",
                table: "Workflows",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "Workflows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_TeamId",
                table: "Workflows",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workflows_Teams_TeamId",
                table: "Workflows",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Create InputConfigs table
            migrationBuilder.CreateTable(
                name: "InputConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false),
                    PollUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PollIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    PollHttpMethod = table.Column<string>(type: "TEXT", nullable: true),
                    PollHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    PollBody = table.Column<string>(type: "TEXT", nullable: true),
                    AuthType = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthCredentials = table.Column<string>(type: "TEXT", nullable: true),
                    IsPollingEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastPollAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WorkflowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputConfigs_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InputConfigs_WorkflowId",
                table: "InputConfigs",
                column: "WorkflowId",
                unique: true);

            // Create OutputConfigs table
            migrationBuilder.CreateTable(
                name: "OutputConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DestinationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiEndpointUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ApiHttpMethod = table.Column<string>(type: "TEXT", nullable: true),
                    ApiHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    ApiAuthType = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiAuthCredentials = table.Column<string>(type: "TEXT", nullable: true),
                    CustomTableName = table.Column<string>(type: "TEXT", nullable: true),
                    AutoCreateTable = table.Column<bool>(type: "INTEGER", nullable: false),
                    TableSchema = table.Column<string>(type: "TEXT", nullable: true),
                    CustomConnectionString = table.Column<string>(type: "TEXT", nullable: true),
                    WorkflowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutputConfigs_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutputConfigs_WorkflowId",
                table: "OutputConfigs",
                column: "WorkflowId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OutputConfigs");
            migrationBuilder.DropTable(name: "InputConfigs");
            
            migrationBuilder.DropForeignKey(name: "FK_Workflows_Teams_TeamId", table: "Workflows");
            migrationBuilder.DropIndex(name: "IX_Workflows_TeamId", table: "Workflows");
            migrationBuilder.DropColumn(name: "TeamId", table: "Workflows");
            migrationBuilder.DropColumn(name: "EntityName", table: "Workflows");
            
            migrationBuilder.DropTable(name: "Teams");
            migrationBuilder.DropTable(name: "Namespaces");
            migrationBuilder.DropTable(name: "Tenants");
        }
    }
}
