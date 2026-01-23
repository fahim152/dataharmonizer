using System;

namespace Harmonizer.DTOs;

public class HarmonizeRequestResponseDto
{
    public int Id { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class HarmonizeRequestCreateDto
{
    public string Payload { get; set; } = string.Empty;
    public string? Source { get; set; }
    public int? WorkflowId { get; set; }
}

