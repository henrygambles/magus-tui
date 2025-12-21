using MagusTui.Models;

namespace MagusTui.Backends;

public interface IChatBackend
{
    string Name { get; }
    Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}
