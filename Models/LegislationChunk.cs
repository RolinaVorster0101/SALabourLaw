namespace SALabourLaw.Models;

public class LegislationChunk
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float[] ContentVector { get; set; } = Array.Empty<float>();
    public string SourceAct { get; set; } = string.Empty;
    public string? SectionNumber { get; set; }
    public string? SectionTitle { get; set; }
    public int PageNumber { get; set; }
    public int ChunkIndex { get; set; }
}