
using Terminal.Gui;

namespace MagusTui;

public static class MenuFactory
{
    public static MenuBar Build(
        ThemeManager themes,
        BackendManager backend,
        ModeManager modes,
        IEnumerable<MenuItem> personas,
        Action onThemeChanged,
        Action onBackendChanged,
        Action onModeChanged,
        Action onShowPalette,
        Action onQuit)
    {
        var themeItems = themes.Names.Select(n => new MenuItem(n, "", () => { themes.Set(n); onThemeChanged(); })).ToArray();
        var backendItems = backend.Names.Select(n => new MenuItem(n, "", () => { backend.Set(n); onBackendChanged(); })).ToArray();
        var modeItems = modes.Names.Select(n => new MenuItem(n, "", () => { modes.Set(n); onModeChanged(); })).ToArray();

        return new MenuBar(new MenuBarItem[] {
            new MenuBarItem("_File", new MenuItem[] {
                new MenuItem("_Command Palette", "", () => onShowPalette(), null, null, Key.CtrlMask | Key.P),
                new MenuItem("_Quit", "", () => onQuit())
            }),
            new MenuBarItem("_View", new MenuItem[] {
                new MenuItem("_Help", "", () => MessageBox.Query(60, 8, "Help", "Press F1 or Ctrl+P.", "OK"))
            }),
            new MenuBarItem("_Backend", backendItems),
            new MenuBarItem("_Themes", themeItems),
            new MenuBarItem("_Mode", modeItems),
            new MenuBarItem("_Persona", personas.ToArray()),
            new MenuBarItem("_Help", new MenuItem[] {
                new MenuItem("_About", "", () => MessageBox.Query(60, 10, "About", "MagusTui v0.2 — local-first terminal workspace.", "OK")),
            }),
        });
    }
}
