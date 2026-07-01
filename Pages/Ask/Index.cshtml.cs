using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SALabourLaw.Services;
using AppChatMessage = SALabourLaw.Models.ChatMessage;
using SALabourLaw.Models;

namespace SALabourLaw.Pages.Ask;

public class IndexModel : PageModel
{
    private readonly RagQueryService _ragQueryService;

    public List<AppChatMessage> Messages { get; set; } = new();

    public IndexModel(RagQueryService ragQueryService)
    {
        _ragQueryService = ragQueryService;
    }

    public void OnGet()
    {
        Messages = HttpContext.Session.GetMessages();
    }

    public async Task<IActionResult> OnPostAsync(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return RedirectToPage();

        Messages = HttpContext.Session.GetMessages();
        Messages.Add(new AppChatMessage { Role = "user", Content = question });

        var answer = await _ragQueryService.QueryLegislationAsync(question);
        Messages.Add(new AppChatMessage { Role = "assistant", Content = answer });

        HttpContext.Session.SetMessages(Messages);
        return RedirectToPage();
    }
}