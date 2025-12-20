
namespace MagusTui;

public sealed class CommandRegistry
{
    private readonly Dictionary<string, (string Title, Action Action)> _cmds = new();

    public IEnumerable<(string Id, string Title)> Items =>
        _cmds.Select(kv => (kv.Key, kv.Value.Title)).OrderBy(x => x.Title);

    public void Add(string title, string id, Action action)
        => _cmds[id] = (title, action);

    public bool TryExecute(string id)
    {
        if (_cmds.TryGetValue(id, out var cmd))
        {
            cmd.Action();
            return true;
        }
        return false;
    }
}
