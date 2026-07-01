using System.Text.Json;
using SALabourLaw.Models;

namespace SALabourLaw.Models;

public static class SessionExtensions
{
    private const string MessagesKey = "chat_messages";

    public static List<ChatMessage> GetMessages(this ISession session)
    {
        var json = session.GetString(MessagesKey);
        if (string.IsNullOrEmpty(json))
            return new List<ChatMessage>();
        return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
    }

    public static void SetMessages(this ISession session, List<ChatMessage> messages)
    {
        session.SetString(MessagesKey, JsonSerializer.Serialize(messages));
    }
}