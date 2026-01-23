using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Harmonizer.Data;
using Harmonizer.Models;

namespace Harmonizer.Services;

public interface IDynamicTableService
{
    /// <summary>
    /// Generates a table name based on tenant hierarchy and entity name
    /// </summary>
    string GenerateTableName(Workflow workflow);
    
    /// <summary>
    /// Generates SQL CREATE TABLE statement from JSON schema
    /// </summary>
    string GenerateCreateTableSql(string tableName, string jsonSchema);
    
    /// <summary>
    /// Generates SQL INSERT statement from harmonized JSON
    /// </summary>
    string GenerateInsertSql(string tableName, string harmonizedJson);
    
    /// <summary>
    /// Creates the dynamic table if it doesn't exist
    /// </summary>
    Task<bool> EnsureTableExistsAsync(Workflow workflow);
    
    /// <summary>
    /// Inserts harmonized data into the dynamic table
    /// </summary>
    Task<bool> InsertHarmonizedDataAsync(Workflow workflow, string harmonizedJson);
    
    /// <summary>
    /// Infers schema from harmonized JSON output
    /// </summary>
    string InferSchemaFromJson(string json);
    
    /// <summary>
    /// Checks if dynamic table exists for workflow
    /// </summary>
    Task<bool> TableExistsAsync(string tableName);
    
    /// <summary>
    /// Gets the table name for a workflow (null if no team/entity assigned)
    /// </summary>
    Task<string?> GetTableNameForWorkflowAsync(int workflowId);
}

public class DynamicTableService : IDynamicTableService
{
    private readonly HarmonizerDbContext _context;
    private readonly ILogger<DynamicTableService> _logger;

    public DynamicTableService(HarmonizerDbContext context, ILogger<DynamicTableService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public string GenerateTableName(Workflow workflow)
    {
        var parts = new List<string>();
        
        if (workflow.Team != null)
        {
            // Load full hierarchy if needed
            var team = workflow.Team;
            if (team.Namespace != null)
            {
                var ns = team.Namespace;
                if (ns.Tenant != null)
                {
                    parts.Add(SanitizeForTableName(ns.Tenant.Slug));
                }
                parts.Add(SanitizeForTableName(ns.Slug));
            }
            parts.Add(SanitizeForTableName(team.Slug));
        }
        
        // Add entity name or workflow name
        var entityName = !string.IsNullOrEmpty(workflow.EntityName) 
            ? workflow.EntityName 
            : workflow.Name;
        parts.Add(SanitizeForTableName(entityName));
        
        return string.Join("_", parts);
    }

    public string GenerateCreateTableSql(string tableName, string jsonSchema)
    {
        var columns = new List<string>
        {
            "id INTEGER PRIMARY KEY AUTOINCREMENT",
            "created_at DATETIME DEFAULT CURRENT_TIMESTAMP",
            "workflow_id INTEGER",
            "request_id INTEGER"
        };

        try
        {
            using var doc = JsonDocument.Parse(jsonSchema);
            var root = doc.RootElement;
            
            // Handle both direct object and wrapped object (e.g., { "harmonizedData": {...} })
            var dataElement = root;
            if (root.TryGetProperty("harmonizedData", out var harmonized))
            {
                dataElement = harmonized;
            }

            ExtractColumns(dataElement, "", columns);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON schema, using generic structure");
            columns.Add("data TEXT"); // Fallback: store entire JSON
        }

        var sql = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (\n  {string.Join(",\n  ", columns)}\n);";
        return sql;
    }

    private void ExtractColumns(JsonElement element, string prefix, List<string> columns)
    {
        if (element.ValueKind != JsonValueKind.Object) return;

        foreach (var property in element.EnumerateObject())
        {
            var columnName = string.IsNullOrEmpty(prefix) 
                ? SanitizeColumnName(property.Name)
                : $"{prefix}_{SanitizeColumnName(property.Name)}";

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.String:
                    columns.Add($"\"{columnName}\" TEXT");
                    break;
                case JsonValueKind.Number:
                    columns.Add($"\"{columnName}\" REAL");
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    columns.Add($"\"{columnName}\" INTEGER"); // SQLite uses INTEGER for boolean
                    break;
                case JsonValueKind.Object:
                    // Flatten nested objects
                    ExtractColumns(property.Value, columnName, columns);
                    break;
                case JsonValueKind.Array:
                    // Store arrays as JSON text
                    columns.Add($"\"{columnName}\" TEXT");
                    break;
                default:
                    columns.Add($"\"{columnName}\" TEXT");
                    break;
            }
        }
    }

    public string GenerateInsertSql(string tableName, string harmonizedJson)
    {
        var columns = new List<string> { "created_at" };
        var values = new List<string> { "CURRENT_TIMESTAMP" };

        try
        {
            using var doc = JsonDocument.Parse(harmonizedJson);
            var root = doc.RootElement;
            
            var dataElement = root;
            if (root.TryGetProperty("harmonizedData", out var harmonized))
            {
                dataElement = harmonized;
            }

            ExtractValues(dataElement, "", columns, values);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse harmonized JSON for insert");
            columns.Add("data");
            values.Add($"'{EscapeSqlString(harmonizedJson)}'");
        }

        var sql = $"INSERT INTO \"{tableName}\" ({string.Join(", ", columns.Select(c => $"\"{c}\""))}) VALUES ({string.Join(", ", values)});";
        return sql;
    }

    private void ExtractValues(JsonElement element, string prefix, List<string> columns, List<string> values)
    {
        if (element.ValueKind != JsonValueKind.Object) return;

        foreach (var property in element.EnumerateObject())
        {
            var columnName = string.IsNullOrEmpty(prefix) 
                ? SanitizeColumnName(property.Name)
                : $"{prefix}_{SanitizeColumnName(property.Name)}";

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.String:
                    columns.Add(columnName);
                    values.Add($"'{EscapeSqlString(property.Value.GetString() ?? "")}'");
                    break;
                case JsonValueKind.Number:
                    columns.Add(columnName);
                    values.Add(property.Value.GetRawText());
                    break;
                case JsonValueKind.True:
                    columns.Add(columnName);
                    values.Add("1");
                    break;
                case JsonValueKind.False:
                    columns.Add(columnName);
                    values.Add("0");
                    break;
                case JsonValueKind.Object:
                    ExtractValues(property.Value, columnName, columns, values);
                    break;
                case JsonValueKind.Array:
                    columns.Add(columnName);
                    values.Add($"'{EscapeSqlString(property.Value.GetRawText())}'");
                    break;
                case JsonValueKind.Null:
                    columns.Add(columnName);
                    values.Add("NULL");
                    break;
            }
        }
    }

    public async Task<bool> EnsureTableExistsAsync(Workflow workflow)
    {
        try
        {
            var tableName = workflow.OutputConfig?.CustomTableName ?? GenerateTableName(workflow);
            var schema = workflow.OutputConfig?.TableSchema ?? workflow.DesiredOutput;
            var createSql = GenerateCreateTableSql(tableName, schema);
            
            await _context.Database.ExecuteSqlRawAsync(createSql);
            _logger.LogInformation("Ensured table exists: {TableName}", tableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create table for workflow {WorkflowId}", workflow.Id);
            return false;
        }
    }

    public async Task<bool> InsertHarmonizedDataAsync(Workflow workflow, string harmonizedJson)
    {
        try
        {
            var tableName = workflow.OutputConfig?.CustomTableName ?? GenerateTableName(workflow);
            
            // Ensure table exists first
            await EnsureTableExistsAsync(workflow);
            
            var insertSql = GenerateInsertSql(tableName, harmonizedJson);
            await _context.Database.ExecuteSqlRawAsync(insertSql);
            
            _logger.LogInformation("Inserted data into table: {TableName}", tableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert data for workflow {WorkflowId}", workflow.Id);
            return false;
        }
    }

    public string InferSchemaFromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return json; // For now, just return the JSON as schema template
        }
        catch
        {
            return "{}";
        }
    }

    private static string SanitizeForTableName(string input)
    {
        if (string.IsNullOrEmpty(input)) return "unnamed";
        
        // Convert to lowercase, replace spaces and special chars with underscore
        var result = input.ToLowerInvariant();
        result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9]", "_");
        result = System.Text.RegularExpressions.Regex.Replace(result, @"_+", "_");
        return result.Trim('_');
    }

    private static string SanitizeColumnName(string input)
    {
        var result = input;
        // Convert camelCase to snake_case
        result = System.Text.RegularExpressions.Regex.Replace(result, @"([a-z])([A-Z])", "$1_$2");
        result = result.ToLowerInvariant();
        result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9_]", "_");
        return result;
    }

    private static string EscapeSqlString(string input)
    {
        return input.Replace("'", "''");
    }
    
    public async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            var sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{EscapeSqlString(tableName)}';";
            var result = await _context.Database.SqlQueryRaw<string>(sql).ToListAsync();
            return result.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if table exists: {TableName}", tableName);
            return false;
        }
    }
    
    public async Task<string?> GetTableNameForWorkflowAsync(int workflowId)
    {
        try
        {
            var workflow = await _context.Workflows
                .Include(w => w.Team)
                    .ThenInclude(t => t!.Namespace)
                        .ThenInclude(n => n.Tenant)
                .FirstOrDefaultAsync(w => w.Id == workflowId);
            
            if (workflow == null) return null;
            if (workflow.TeamId == null || string.IsNullOrEmpty(workflow.EntityName)) return null;
            
            var tableName = GenerateTableName(workflow);
            var exists = await TableExistsAsync(tableName);
            
            return exists ? tableName : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get table name for workflow {WorkflowId}", workflowId);
            return null;
        }
    }
}
