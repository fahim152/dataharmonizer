namespace Harmonizer.Models;

public class Workflow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = "JSON"; // JSON, TEXT, VARIABLE
    public string ApiKey { get; set; } = string.Empty; // Unique key for API access
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Multi-tenancy: Optional team association
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    
    // Entity name for dynamic table creation (e.g., "sensor_readings", "orders")
    public string? EntityName { get; set; }
    
    // Outbound API configuration
    public bool OutboundApiEnabled { get; set; } = false;
    public string? OutboundApiUrl { get; set; }
    public string? OutboundApiHeaders { get; set; } // JSON string of headers
    
    // Navigation properties
    public ICollection<HarmonizeRequest> HarmonizeRequests { get; set; } = new List<HarmonizeRequest>();
    public InputConfig? InputConfig { get; set; }
    public OutputConfig? OutputConfig { get; set; }
}
