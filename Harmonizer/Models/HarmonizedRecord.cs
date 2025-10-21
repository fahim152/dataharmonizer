namespace Harmonizer.Models;

public class HarmonizedRecord
{
    public int Id { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public string? Source { get; set; }
    public string? HarmonizedJson { get; set; }
    public int HarmonizeRequestId { get; set; } // Foreign key
    public HarmonizeRequest HarmonizeRequest { get; set; } = null!; // Navigation property
}
