using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SALabourLaw.Services;

namespace SALabourLaw.Pages.Contracts;

public class UploadModel : PageModel
{
    private readonly ChunkingService _chunkingService;
    private readonly EmbeddingService _embeddingService;
    private readonly VectorSearchService _vectorSearchService;
    private readonly ILogger<UploadModel> _logger;

    public string? ErrorMessage { get; set; }

    public UploadModel(
        ChunkingService chunkingService,
        EmbeddingService embeddingService,
        VectorSearchService vectorSearchService,
        ILogger<UploadModel> logger)
    {
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _logger = logger;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(IFormFile contractFile)
    {
        if (contractFile == null || contractFile.Length == 0)
        {
            ErrorMessage = "Please select a file to upload.";
            return Page();
        }

        if (contractFile.Length > 10 * 1024 * 1024)
        {
            ErrorMessage = "File size exceeds the 10MB limit.";
            return Page();
        }

        var extension = Path.GetExtension(contractFile.FileName).ToLowerInvariant();
        if (extension != ".pdf" && extension != ".txt")
        {
            ErrorMessage = "Only PDF and TXT files are accepted.";
            return Page();
        }

        try
        {
            var sessionId = Guid.NewGuid().ToString();
            string text;

            if (extension == ".txt")
            {
                using var reader = new StreamReader(contractFile.OpenReadStream());
                text = await reader.ReadToEndAsync();
            }
            else
            {
                ErrorMessage = "PDF parsing will be added in the next update. Please upload a TXT file for now.";
                return Page();
            }

            var chunks = _chunkingService.ChunkTextWithIndex(text);

            foreach (var (chunkText, chunkIndex) in chunks)
            {
                var vector = await _embeddingService.GetEmbeddingAsync(chunkText);
                var chunk = new Models.ContractChunk
                {
                    Id = $"{sessionId}-chunk-{chunkIndex}",
                    Content = chunkText,
                    ContentVector = vector,
                    SessionId = sessionId,
                    FileName = contractFile.FileName,
                    PageNumber = 0,
                    ChunkIndex = chunkIndex,
                    UploadedAt = DateTimeOffset.UtcNow
                };
                await _vectorSearchService.UpsertContractChunkAsync(chunk);
            }

            _logger.LogInformation("Contract uploaded and indexed for session {SessionId}", sessionId);
            return RedirectToPage("/Contracts/Review", new { sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contract upload");
            ErrorMessage = "An error occurred while processing your file. Please try again.";
            return Page();
        }
    }
}