namespace Harmonizer.DTOs;

public class WorkflowDto
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
    public int? TeamId { get; set; }
    public string? EntityName { get; set; }
    public bool OutboundApiEnabled { get; set; }
    public string? OutboundApiUrl { get; set; }
    public string? OutboundApiHeaders { get; set; }
}

public class WorkflowCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = "JSON";
    public int? TeamId { get; set; }
    public string? EntityName { get; set; }
    public bool OutboundApiEnabled { get; set; }
    public string? OutboundApiUrl { get; set; }
    public string? OutboundApiHeaders { get; set; }
}

public class WorkflowUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ContextPrompt { get; set; }
    public string? DesiredOutput { get; set; }
    public string? DesiredOutputType { get; set; }
    public bool? IsActive { get; set; }
    public int? TeamId { get; set; }
    public string? EntityName { get; set; }
    public bool? OutboundApiEnabled { get; set; }
    public string? OutboundApiUrl { get; set; }
    public string? OutboundApiHeaders { get; set; }
}

public class WorkflowWebhookRequestDto
{
    public object? Payload { get; set; }
    public string? Source { get; set; }
}

public class WorkflowWebhookResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? RequestId { get; set; }
    public int? RecordId { get; set; }
    public object? HarmonizedData { get; set; }
    public string? Error { get; set; }
}
