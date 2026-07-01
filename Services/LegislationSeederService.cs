using SALabourLaw.Models;
using UglyToad.PdfPig;

namespace SALabourLaw.Services;

public class LegislationSeederService
{
    private readonly ChunkingService _chunkingService;
    private readonly EmbeddingService _embeddingService;
    private readonly VectorSearchService _vectorSearchService;
    private readonly ILogger<LegislationSeederService> _logger;

    public LegislationSeederService(
        ChunkingService chunkingService,
        EmbeddingService embeddingService,
        VectorSearchService vectorSearchService,
        ILogger<LegislationSeederService> logger)
    {
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _logger = logger;
    }

    public async Task SeedAsync(string filePath, string sourceAct)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Seed file not found: {FilePath}", filePath);
            return;
        }

        _logger.LogInformation("Seeding {SourceAct} from {FilePath}", sourceAct, filePath);

        var text = ExtractTextFromPdf(filePath);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("No text extracted from {FilePath}", filePath);
            return;
        }

        var chunks = _chunkingService.ChunkTextWithIndex(text);
        _logger.LogInformation("Created {ChunkCount} chunks for {SourceAct}", chunks.Count, sourceAct);

        for (int i = 0; i < chunks.Count; i++)
        {
            var (chunkText, chunkIndex) = chunks[i];

            var vector = await _embeddingService.GetEmbeddingAsync(chunkText);

            var chunk = new LegislationChunk
            {
                Id = $"{sourceAct.ToLower().Replace(" ", "-")}-chunk-{chunkIndex}",
                Content = chunkText,
                ContentVector = vector,
                SourceAct = sourceAct,
                SectionNumber = null,
                SectionTitle = null,
                PageNumber = 0,
                ChunkIndex = chunkIndex
            };

            await _vectorSearchService.UpsertLegislationChunkAsync(chunk);
            _logger.LogInformation("Seeded chunk {Index}/{Total} for {SourceAct}",
                i + 1, chunks.Count, sourceAct);
        }

        _logger.LogInformation("Completed seeding {SourceAct}", sourceAct);
    }

    private string ExtractTextFromPdf(string filePath)
    {
        var sb = new System.Text.StringBuilder();

        using var document = PdfDocument.Open(filePath);
        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }
}