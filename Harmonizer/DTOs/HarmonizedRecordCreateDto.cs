namespace Harmonizer.DTOs;

public class HarmonizedRecordCreateDto
{
    public string RawJson { get; set; } = string.Empty;
    public string? Source { get; set; }
    public int HarmonizeRequestId { get; set; }
}

