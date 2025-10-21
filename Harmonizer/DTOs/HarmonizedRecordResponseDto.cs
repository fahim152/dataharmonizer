using System;

namespace Harmonizer.DTOs;

public class HarmonizedRecordResponseDto
{
    public int Id { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? HarmonizedJson { get; set; }
    public int HarmonizeRequestId { get; set; }
    public DateTime ReceivedAt { get; set; }
}

