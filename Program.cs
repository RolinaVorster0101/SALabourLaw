using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using SALabourLaw.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Register Azure OpenAI client
builder.Services.AddSingleton(new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
    new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!)));

// Register Azure AI Search index client
builder.Services.AddSingleton(new SearchIndexClient(
    new Uri(builder.Configuration["AzureSearch:Endpoint"]!),
    new AzureKeyCredential(builder.Configuration["AzureSearch:ApiKey"]!)));

builder.Services.AddSingleton<ChunkingService>();

builder.Services.AddSingleton<EmbeddingService>();

builder.Services.AddSingleton<VectorSearchService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();