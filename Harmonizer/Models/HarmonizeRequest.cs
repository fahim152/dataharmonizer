using System;

namespace Harmonizer.Models;

public class HarmonizeRequest
{
    public int Id { get; set; }
    public string Payload { get; set; } = string.Empty; // Always a string, no validation
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Optional: Link to workflow that processed this request
    public int? WorkflowId { get; set; }
    public Workflow? Workflow { get; set; }
    
    public ICollection<HarmonizedRecord> HarmonizedRecords { get; set; } = new List<HarmonizedRecord>();
}

