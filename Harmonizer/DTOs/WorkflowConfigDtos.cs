using Harmonizer.Models;

namespace Harmonizer.DTOs;

// Input Config DTOs
public class InputConfigDto
{
    public int Id { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? PollUrl { get; set; }
    public int? PollIntervalSeconds { get; set; }
    public string? PollHttpMethod { get; set; }
    public string? PollHeaders { get; set; }
    public string? PollBody { get; set; }
    public string AuthType { get; set; } = "None";
    public bool IsPollingEnabled { get; set; }
    public DateTime? LastPollAt { get; set; }
}

public class InputConfigCreateDto
{
    public string SourceType { get; set; } = "Manual";
    public string? PollUrl { get; set; }
    public int? PollIntervalSeconds { get; set; }
    public string? PollHttpMethod { get; set; }
    public string? PollHeaders { get; set; }
    public string? PollBody { get; set; }
    public string AuthType { get; set; } = "None";
    public string? AuthCredentials { get; set; }
    public bool IsPollingEnabled { get; set; }
}

// Output Config DTOs
public class OutputConfigDto
{
    public int Id { get; set; }
    public string DestinationType { get; set; } = "Database";
    public string? ApiEndpointUrl { get; set; }
    public string? ApiHttpMethod { get; set; }
    public string? ApiHeaders { get; set; }
    public string ApiAuthType { get; set; } = "None";
    public string? CustomTableName { get; set; }
    public bool AutoCreateTable { get; set; }
    public string? TableSchema { get; set; }
    public bool HasCustomConnection { get; set; }
    
    // Computed table name preview
    public string? GeneratedTableName { get; set; }
}

public class OutputConfigCreateDto
{
    public string DestinationType { get; set; } = "Database";
    public string? ApiEndpointUrl { get; set; }
    public string? ApiHttpMethod { get; set; }
    public string? ApiHeaders { get; set; }
    public string ApiAuthType { get; set; } = "None";
    public string? ApiAuthCredentials { get; set; }
    public string? CustomTableName { get; set; }
    public bool AutoCreateTable { get; set; } = true;
    public string? CustomConnectionString { get; set; }
}

// Extended Workflow DTO with configs
public class WorkflowFullDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = "JSON";
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Multi-tenancy
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? NamespaceName { get; set; }
    public string? TenantName { get; set; }
    public string? EntityName { get; set; }
    
    // Configs
    public InputConfigDto? InputConfig { get; set; }
    public OutputConfigDto? OutputConfig { get; set; }
    
    // Computed
    public string WebhookUrl { get; set; } = string.Empty;
    public string? GeneratedTableName { get; set; }
    public int RequestCount { get; set; }
}

public class WorkflowCreateFullDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = "JSON";
    public int? TeamId { get; set; }
    public string? EntityName { get; set; }
    public InputConfigCreateDto? InputConfig { get; set; }
    public OutputConfigCreateDto? OutputConfig { get; set; }
}
