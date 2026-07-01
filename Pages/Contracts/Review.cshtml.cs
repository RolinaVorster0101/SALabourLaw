using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SALabourLaw.Services;
using AppChatMessage = SALabourLaw.Models.ChatMessage;
using SALabourLaw.Models;

namespace SALabourLaw.Pages.Contracts;

public class ReviewModel : PageModel
{
    private readonly RagQueryService _ragQueryService;

    public List<AppChatMessage> Messages { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;

    public ReviewModel(RagQueryService ragQueryService)
    {
        _ragQueryService = ragQueryService;
    }

    public void OnGet(string sessionId)
    {
        SessionId = sessionId;
        Messages = HttpContext.Session.GetMessages($"contract_{sessionId}");
    }

    public async Task<IActionResult> OnPostAsync(string question, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(question))
            return RedirectToPage(new { sessionId });

        SessionId = sessionId;
        var sessionKey = $"contract_{sessionId}";
        Messages = HttpContext.Session.GetMessages(sessionKey);
        Messages.Add(new AppChatMessage { Role = "user", Content = question });

        var answer = await _ragQueryService.QueryContractAsync(question, sessionId);
        Messages.Add(new AppChatMessage { Role = "assistant", Content = answer });

        HttpContext.Session.SetMessages(Messages, sessionKey);
        return RedirectToPage(new { sessionId });
    }
}