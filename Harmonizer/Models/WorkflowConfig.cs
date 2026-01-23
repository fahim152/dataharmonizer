namespace Harmonizer.Models;

/// <summary>
/// Configuration for data input sources
/// </summary>
public class InputConfig
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of input source: Manual, Webhook, ApiPoll, FileUpload
    /// </summary>
    public InputSourceType SourceType { get; set; } = InputSourceType.Manual;
    
    /// <summary>
    /// For ApiPoll: The URL to poll for data
    /// </summary>
    public string? PollUrl { get; set; }
    
    /// <summary>
    /// For ApiPoll: Polling interval in seconds
    /// </summary>
    public int? PollIntervalSeconds { get; set; }
    
    /// <summary>
    /// For ApiPoll: HTTP method (GET, POST)
    /// </summary>
    public string? PollHttpMethod { get; set; } = "GET";
    
    /// <summary>
    /// For ApiPoll: Request headers as JSON
    /// </summary>
    public string? PollHeaders { get; set; }
    
    /// <summary>
    /// For ApiPoll: Request body for POST requests
    /// </summary>
    public string? PollBody { get; set; }
    
    /// <summary>
    /// Authentication type: None, ApiKey, Bearer, Basic
    /// </summary>
    public AuthType AuthType { get; set; } = AuthType.None;
    
    /// <summary>
    /// Authentication credentials (encrypted)
    /// </summary>
    public string? AuthCredentials { get; set; }
    
    /// <summary>
    /// Whether polling is currently enabled
    /// </summary>
    public bool IsPollingEnabled { get; set; } = false;
    
    /// <summary>
    /// Last successful poll timestamp
    /// </summary>
    public DateTime? LastPollAt { get; set; }
    
    // Navigation
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
}

/// <summary>
/// Configuration for data output destinations
/// </summary>
public class OutputConfig
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type of output destination: Database, Api, Both
    /// </summary>
    public OutputDestinationType DestinationType { get; set; } = OutputDestinationType.Database;
    
    // --- API Output Settings ---
    
    /// <summary>
    /// For Api: The endpoint URL to send harmonized data
    /// </summary>
    public string? ApiEndpointUrl { get; set; }
    
    /// <summary>
    /// For Api: HTTP method (POST, PUT, PATCH)
    /// </summary>
    public string? ApiHttpMethod { get; set; } = "POST";
    
    /// <summary>
    /// For Api: Request headers as JSON
    /// </summary>
    public string? ApiHeaders { get; set; }
    
    /// <summary>
    /// For Api: Authentication type
    /// </summary>
    public AuthType ApiAuthType { get; set; } = AuthType.None;
    
    /// <summary>
    /// For Api: Authentication credentials
    /// </summary>
    public string? ApiAuthCredentials { get; set; }
    
    // --- Database Output Settings ---
    
    /// <summary>
    /// For Database: Custom table name (overrides auto-generated)
    /// </summary>
    public string? CustomTableName { get; set; }
    
    /// <summary>
    /// For Database: Whether to auto-create table from schema
    /// </summary>
    public bool AutoCreateTable { get; set; } = true;
    
    /// <summary>
    /// For Database: The generated/stored schema for the output table
    /// </summary>
    public string? TableSchema { get; set; }
    
    /// <summary>
    /// For Database: Connection string (if different from default)
    /// </summary>
    public string? CustomConnectionString { get; set; }
    
    // Navigation
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
}

public enum InputSourceType
{
    Manual,      // User manually submits data via UI
    Webhook,     // Data comes via webhook endpoint
    ApiPoll,     // System polls an external API
    FileUpload   // User uploads a file
}

public enum OutputDestinationType
{
    Database,    // Save to database table
    Api,         // Send to external API
    Both         // Save to DB and send to API
}

public enum AuthType
{
    None,
    ApiKey,
    Bearer,
    Basic
}
