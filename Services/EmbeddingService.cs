using Azure.AI.OpenAI;
using OpenAI.Embeddings;

namespace SALabourLaw.Services;

public class EmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public EmbeddingService(AzureOpenAIClient openAIClient, IConfiguration configuration)
    {
        var deploymentName = configuration["AzureOpenAI:EmbeddingDeployment"]!;
        _embeddingClient = openAIClient.GetEmbeddingClient(deploymentName);
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var result = await _embeddingClient.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
    {
        var embeddings = new List<float[]>();
        foreach (var text in texts)
        {
            var embedding = await GetEmbeddingAsync(text);
            embeddings.Add(embedding);
        }
        return embeddings;
    }
}