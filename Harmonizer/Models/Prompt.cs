namespace Harmonizer.Models;

public class Prompt
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Friendly name for dropdown
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = string.Empty; // e.g., "JSON", "XML", "TEXT"
    public string? Description { get; set; } // Optional description
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

