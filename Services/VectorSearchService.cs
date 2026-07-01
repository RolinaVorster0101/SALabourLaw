using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using SALabourLaw.Models;

namespace SALabourLaw.Services;

public class VectorSearchService
{
    private readonly SearchIndexClient _indexClient;
    private readonly IConfiguration _configuration;

    public VectorSearchService(SearchIndexClient indexClient, IConfiguration configuration)
    {
        _indexClient = indexClient;
        _configuration = configuration;
    }

    public async Task UpsertLegislationChunkAsync(LegislationChunk chunk)
    {
        var indexName = _configuration["AzureSearch:LegislationIndex"]!;
        var searchClient = _indexClient.GetSearchClient(indexName);

        var document = new
        {
            id = chunk.Id,
            content = chunk.Content,
            contentVector = chunk.ContentVector,
            sourceAct = chunk.SourceAct,
            sectionNumber = chunk.SectionNumber,
            sectionTitle = chunk.SectionTitle,
            pageNumber = chunk.PageNumber,
            chunkIndex = chunk.ChunkIndex
        };

        await searchClient.MergeOrUploadDocumentsAsync(new[] { document });
    }

    public async Task UpsertContractChunkAsync(ContractChunk chunk)
    {
        var indexName = _configuration["AzureSearch:ContractsIndex"]!;
        var searchClient = _indexClient.GetSearchClient(indexName);

        var document = new
        {
            id = chunk.Id,
            content = chunk.Content,
            contentVector = chunk.ContentVector,
            sessionId = chunk.SessionId,
            fileName = chunk.FileName,
            pageNumber = chunk.PageNumber,
            chunkIndex = chunk.ChunkIndex,
            uploadedAt = chunk.UploadedAt
        };

        await searchClient.MergeOrUploadDocumentsAsync(new[] { document });
    }

    public async Task<List<RetrievedChunk>> SearchLegislationAsync(float[] queryVector, int topK = 5)
    {
        var indexName = _configuration["AzureSearch:LegislationIndex"]!;
        var searchClient = _indexClient.GetSearchClient(indexName);

        var searchOptions = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(queryVector)
                    {
                        KNearestNeighborsCount = topK,
                        Fields = { "contentVector" }
                    }
                }
            },
            Size = topK,
            Select = { "id", "content", "sourceAct", "sectionNumber", "sectionTitle", "pageNumber" }
        };

        var response = await searchClient.SearchAsync<SearchDocument>(null, searchOptions);
        var results = new List<RetrievedChunk>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(new RetrievedChunk
            {
                Content = result.Document["content"]?.ToString() ?? "",
                SourceAct = result.Document["sourceAct"]?.ToString(),
                SectionNumber = result.Document["sectionNumber"]?.ToString(),
                SectionTitle = result.Document["sectionTitle"]?.ToString(),
                Score = result.Score ?? 0,
                Source = "legislation"
            });
        }

        return results;
    }

    public async Task<List<RetrievedChunk>> SearchContractsAsync(float[] queryVector, string sessionId, int topK = 5)
    {
        var indexName = _configuration["AzureSearch:ContractsIndex"]!;
        var searchClient = _indexClient.GetSearchClient(indexName);

        var searchOptions = new SearchOptions
        {
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(queryVector)
                    {
                        KNearestNeighborsCount = topK,
                        Fields = { "contentVector" }
                    }
                }
            },
            Filter = $"sessionId eq '{sessionId}'",
            Size = topK,
            Select = { "id", "content", "fileName", "pageNumber" }
        };

        var response = await searchClient.SearchAsync<SearchDocument>(null, searchOptions);
        var results = new List<RetrievedChunk>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(new RetrievedChunk
            {
                Content = result.Document["content"]?.ToString() ?? "",
                SourceAct = null,
                SectionNumber = null,
                SectionTitle = null,
                Score = result.Score ?? 0,
                Source = "contract"
            });
        }

        return results;
    }

    public async Task DeleteContractSessionAsync(string sessionId)
    {
        var indexName = _configuration["AzureSearch:ContractsIndex"]!;
        var searchClient = _indexClient.GetSearchClient(indexName);

        var searchOptions = new SearchOptions
        {
            Filter = $"sessionId eq '{sessionId}'",
            Select = { "id" }
        };

        var response = await searchClient.SearchAsync<SearchDocument>("*", searchOptions);
        var ids = new List<string>();

        await foreach (var result in response.Value.GetResultsAsync())
            ids.Add(result.Document["id"]!.ToString()!);

        if (ids.Any())
        {
            var documents = ids.Select(id => new { id }).ToList();
            await searchClient.DeleteDocumentsAsync("id", ids);
        }
    }
}