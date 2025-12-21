
using System.Text.Json;
using Terminal.Gui;
using MagusTui.Config;

namespace MagusTui;

public sealed class ThemeManager
{
    private readonly List<ThemeSpec> _themes = new();
    private int _index = 0;

    public IReadOnlyList<string> Names => _themes.Select(t => t.Name).ToList();
    public string CurrentName => _themes.Count == 0 ? "Default" : _themes[_index].Name;

    public void LoadAll()
    {
        _themes.Clear();

        var baseDir = AppContext.BaseDirectory;
        var builtIn = Path.Combine(baseDir, "Themes");
        var community = MagusHome.ThemesDir;

        var files = new List<string>();
        if (Directory.Exists(builtIn))
            files.AddRange(Directory.GetFiles(builtIn, "*.json"));
        if (Directory.Exists(community))
            files.AddRange(Directory.GetFiles(community, "*.json"));

        foreach (var file in files.OrderBy(f => f))
        {
            try
            {
                var json = File.ReadAllText(file);
                var spec = JsonSerializer.Deserialize<ThemeSpec>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (spec != null && !string.IsNullOrWhiteSpace(spec.Name))
                {
                    // Prefer later entries (community) if name collides
                    _themes.RemoveAll(t => t.Name.Equals(spec.Name, StringComparison.OrdinalIgnoreCase));
                    _themes.Add(spec);
                }
            }
            catch
            {
                // ignore bad themes
            }
        }

        if (_themes.Count == 0)
            _themes.Add(ThemeSpec.Fallback());

        _index = Math.Clamp(_index, 0, _themes.Count - 1);
    }

    public void Cycle()
    {
        if (_themes.Count == 0) return;
        _index = (_index + 1) % _themes.Count;
    }

    public void Set(string name)
    {
        var idx = _themes.FindIndex(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _index = idx;
    }

    public void ApplyCurrent()
    {
        if (_themes.Count == 0) return;
        Apply(_themes[_index]);
    }

    private static void Apply(ThemeSpec spec)
    {
        // Base scheme
        Colors.Base.Normal = Attr(spec.BaseFg(), spec.BaseBg());
        Colors.Base.Focus = Attr(spec.AccentFg(), spec.AccentBg());
        Colors.Base.HotNormal = Attr(spec.HotFg(), spec.BaseBg());
        Colors.Base.HotFocus = Attr(spec.HotFg(), spec.AccentBg());
        Colors.Base.Disabled = Attr(spec.DisabledFg(), spec.BaseBg());

        // Menu scheme
        Colors.Menu.Normal = Attr(spec.BaseFg(), spec.BaseBg());
        Colors.Menu.Focus = Attr(spec.AccentFg(), spec.AccentBg());
        Colors.Menu.HotNormal = Attr(spec.HotFg(), spec.BaseBg());
        Colors.Menu.HotFocus = Attr(spec.HotFg(), spec.AccentBg());
        Colors.Menu.Disabled = Attr(spec.DisabledFg(), spec.BaseBg());

        // Dialog scheme
        Colors.Dialog.Normal = Attr(spec.BaseFg(), spec.BaseBg());
        Colors.Dialog.Focus = Attr(spec.AccentFg(), spec.AccentBg());
        Colors.Dialog.HotNormal = Attr(spec.HotFg(), spec.BaseBg());
        Colors.Dialog.HotFocus = Attr(spec.HotFg(), spec.AccentBg());
        Colors.Dialog.Disabled = Attr(spec.DisabledFg(), spec.BaseBg());

        // Error scheme
        Colors.Error.Normal = Attr(Color.White, Color.Red);
        Colors.Error.Focus = Attr(Color.White, Color.Red);
        Colors.Error.HotNormal = Attr(Color.BrightYellow, Color.Red);
        Colors.Error.HotFocus = Attr(Color.BrightYellow, Color.Red);
        Colors.Error.Disabled = Attr(Color.Gray, Color.Red);

        // apply immediately
        Application.Refresh();
    }

    private static Terminal.Gui.Attribute Attr(Color fg, Color bg) =>
        Application.Driver.MakeAttribute(fg, bg);
}
