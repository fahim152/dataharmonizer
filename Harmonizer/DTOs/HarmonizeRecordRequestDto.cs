
namespace Harmonizer.DTOs;

public class HarmonizeRecordRequestDto
{
    public string ContextPrompt { get; set; } = string.Empty;
    public string? DesiredSchema { get; set; }
}