namespace Harmonizer.DTOs;

public class PromptResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PromptCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string ContextPrompt { get; set; } = string.Empty;
    public string DesiredOutput { get; set; } = string.Empty;
    public string DesiredOutputType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

