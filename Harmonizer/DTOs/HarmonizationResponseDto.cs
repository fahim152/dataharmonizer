using System.Text.Json;

namespace Harmonizer.DTOs;

public class HarmonizationResponseDto
{
    public int Id { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Source { get; set; }
    public JsonElement RawPayload { get; set; }
    public JsonElement HarmonizedPayload { get; set; }
}
