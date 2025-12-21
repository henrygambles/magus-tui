using MagusTui.Models;

namespace MagusTui.Backends;

public class OfflineBackend : IChatBackend
{
    public string Name => "Offline";

    public Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        var last = messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
        return Task.FromResult($"(offline demo) I can't reach a model right now, but I received: {last}");
    }
}
