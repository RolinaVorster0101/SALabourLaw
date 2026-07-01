namespace SALabourLaw.Services;

public class ChunkingService
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    public ChunkingService(IConfiguration configuration)
    {
        _chunkSize = configuration.GetValue<int>("Chunking:ChunkSize", 500);
        _overlap = configuration.GetValue<int>("Chunking:Overlap", 50);
    }

    public List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int index = 0;
        while (index < words.Length)
        {
            var chunk = words.Skip(index).Take(_chunkSize);
            chunks.Add(string.Join(" ", chunk));
            index += _chunkSize - _overlap;
        }

        return chunks;
    }

    public List<(string Text, int ChunkIndex)> ChunkTextWithIndex(string text)
    {
        var rawChunks = ChunkText(text);
        return rawChunks.Select((chunk, i) => (chunk, i)).ToList();
    }
}