
using System.Text.Json.Serialization;
using Terminal.Gui;

namespace MagusTui;

public sealed class ThemeSpec
{
    [JsonPropertyName("name")] public string Name { get; set; } = "Theme";
    [JsonPropertyName("base")] public ThemePair Base { get; set; } = new();
    [JsonPropertyName("accent")] public ThemePair Accent { get; set; } = new();
    [JsonPropertyName("hot")] public ThemePair Hot { get; set; } = new();
    [JsonPropertyName("status")] public ThemePair Status { get; set; } = new();
    [JsonPropertyName("disabled")] public ThemePair Disabled { get; set; } = new() { Fg = "Gray", Bg = "Black" };

    public Color BaseFg() => ThemeColor.Parse(Base.Fg);
    public Color BaseBg() => ThemeColor.Parse(Base.Bg);
    public Color AccentFg() => ThemeColor.Parse(Accent.Fg);
    public Color AccentBg() => ThemeColor.Parse(Accent.Bg);
    public Color HotFg() => ThemeColor.Parse(Hot.Fg);
    public Color HotBg() => ThemeColor.Parse(Hot.Bg);
    public Color StatusFg() => ThemeColor.Parse(Status.Fg);
    public Color StatusBg() => ThemeColor.Parse(Status.Bg);
    public Color DisabledFg() => ThemeColor.Parse(Disabled.Fg);

    public static ThemeSpec Fallback() => new()
    {
        Name = "Default",
        Base = new ThemePair { Fg = "White", Bg = "Black" },
        Accent = new ThemePair { Fg = "BrightCyan", Bg = "Black" },
        Hot = new ThemePair { Fg = "BrightYellow", Bg = "Black" },
        Status = new ThemePair { Fg = "White", Bg = "Black" },
        Disabled = new ThemePair { Fg = "Gray", Bg = "Black" }
    };
}

public sealed class ThemePair
{
    [JsonPropertyName("fg")] public string Fg { get; set; } = "White";
    [JsonPropertyName("bg")] public string Bg { get; set; } = "Black";
}
