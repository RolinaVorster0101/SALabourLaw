namespace SALabourLaw.Models;

public class ContractChunk
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float[] ContentVector { get; set; } = Array.Empty<float>();
    public string SessionId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int ChunkIndex { get; set; }
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}