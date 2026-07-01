namespace SALabourLaw.Models;

public class RetrievedChunk
{
    public string Content { get; set; } = string.Empty;
    public string? SourceAct { get; set; }
    public string? SectionNumber { get; set; }
    public string? SectionTitle { get; set; }
    public double Score { get; set; }
    public string Source { get; set; } = string.Empty;
}