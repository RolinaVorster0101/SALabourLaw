using System.Text.Json;
using SALabourLaw.Models;

namespace SALabourLaw.Models;

public static class SessionExtensions
{
    private const string DefaultMessagesKey = "chat_messages";

    public static List<ChatMessage> GetMessages(this ISession session, string key = DefaultMessagesKey)
    {
        var json = session.GetString(key);
        if (string.IsNullOrEmpty(json))
            return new List<ChatMessage>();
        return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
    }

    public static void SetMessages(this ISession session, List<ChatMessage> messages, string key = DefaultMessagesKey)
    {
        session.SetString(key, JsonSerializer.Serialize(messages));
    }
}