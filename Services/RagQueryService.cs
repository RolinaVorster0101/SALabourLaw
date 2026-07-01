using Azure.AI.OpenAI;
using OpenAI.Chat;
using SALabourLaw.Models;

namespace SALabourLaw.Services;

public class RagQueryService
{
    private readonly EmbeddingService _embeddingService;
    private readonly VectorSearchService _vectorSearchService;
    private readonly ChatClient _chatClient;

    public RagQueryService(
        EmbeddingService embeddingService,
        VectorSearchService vectorSearchService,
        AzureOpenAIClient openAIClient,
        IConfiguration configuration)
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        var deploymentName = configuration["AzureOpenAI:ChatDeployment"]!;
        _chatClient = openAIClient.GetChatClient(deploymentName);
    }

    public async Task<string> QueryLegislationAsync(string question)
    {
        var queryVector = await _embeddingService.GetEmbeddingAsync(question);
        var chunks = await _vectorSearchService.SearchLegislationAsync(queryVector);

        if (!chunks.Any())
            return "I could not find relevant information in the legislation to answer your question.";

        var context = BuildLegislationContext(chunks);
        var prompt = BuildLegislationPrompt(question, context);

        var response = await _chatClient.CompleteChatAsync(new List<ChatMessage>
{
    new SystemChatMessage(@"You are a South African employment law assistant. 
        Answer questions using only the legislative excerpts provided. 
        Always cite the specific act and section number for each claim you make.
        If the provided excerpts do not contain enough information to answer the question, say so clearly.
        Do not provide legal advice — remind users to consult a qualified labour attorney for their specific situation."),
    new UserChatMessage(prompt)
});

        return response.Value.Content[0].Text;
    }

    public async Task<string> QueryContractAsync(string question, string sessionId)
    {
        var queryVector = await _embeddingService.GetEmbeddingAsync(question);

        var legislationChunks = await _vectorSearchService.SearchLegislationAsync(queryVector);
        var contractChunks = await _vectorSearchService.SearchContractsAsync(queryVector, sessionId);

        if (!legislationChunks.Any() && !contractChunks.Any())
            return "I could not find relevant information in either the legislation or your contract to answer your question.";

        var context = BuildDualContext(legislationChunks, contractChunks);
        var prompt = BuildContractPrompt(question, context);

        var response = await _chatClient.CompleteChatAsync(new List<ChatMessage>
{
    new SystemChatMessage(@"You are a South African employment law compliance assistant.
        You will be given excerpts from both an employment contract and relevant legislation (BCEA, LRA, CCMA guidance).
        Compare the contract clauses against the legislation and identify:
        1. Compliant clauses — where the contract meets or exceeds legislative requirements
        2. Non-compliant clauses — where the contract falls below legislative minimums
        3. Ambiguous clauses — where compliance is unclear or requires legal interpretation
        Always cite both the contract clause and the specific legislative section.
        Do not provide legal advice — remind users to consult a qualified labour attorney for their specific situation."),
    new UserChatMessage(prompt)
});

        return response.Value.Content[0].Text;
    }

    private string BuildLegislationContext(List<RetrievedChunk> chunks)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var chunk in chunks)
        {
            sb.AppendLine($"[{chunk.SourceAct} Section {chunk.SectionNumber}]");
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string BuildDualContext(List<RetrievedChunk> legislationChunks, List<RetrievedChunk> contractChunks)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("CONTRACT EXCERPTS:");
        foreach (var chunk in contractChunks)
        {
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }

        sb.AppendLine("LEGISLATION EXCERPTS:");
        foreach (var chunk in legislationChunks)
        {
            sb.AppendLine($"[{chunk.SourceAct} Section {chunk.SectionNumber}]");
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string BuildLegislationPrompt(string question, string context)
    {
        return $"Using only the following legislative excerpts, answer this question: {question}\n\n{context}";
    }

    private string BuildContractPrompt(string question, string context)
    {
        return $"Using the following contract and legislation excerpts, answer this question: {question}\n\n{context}";
    }
}