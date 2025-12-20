
namespace MagusTui;

public sealed class ModeManager
{
    private readonly List<string> _names = new()
    {
        "Explore",
        "Build",
        "Reflect"
    };

    private int _index = 0;

    public IReadOnlyList<string> Names => _names;
    public string CurrentName => _names[_index];

    public void Cycle() => _index = (_index + 1) % _names.Count;

    public void Set(string name)
    {
        var idx = _names.FindIndex(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _index = idx;
    }
}
