
using System.Text;
using Terminal.Gui;

namespace MagusTui;

public sealed class MagusApp
{
    private readonly ThemeManager _themes;
    private readonly BackendManager _backend;
    private readonly ModeManager _modes;
    private readonly CommandRegistry _commands;

    private Window? _main;
    private StatusBar? _status;
    private TabView? _tabs;
    private TabView.Tab? _chatTab;
    private TabView.Tab? _searchTab;
    private TabView.Tab? _notesTab;
    private TabView.Tab? _logsTab;

    // Notes
    private NotesWorkspace? _notes;

    // Chat
    private ChatPane? _chat;

    // Search
    private SearchPane? _search;

    // Logs
    private LogPane? _logs;

    public MagusApp()
    {
        _themes = new ThemeManager();
        _backend = new BackendManager();
        _modes = new ModeManager();
        _commands = new CommandRegistry();
    }

    public void Run()
    {
        // Splash first
        ShowSplash(() =>
        {
            BuildUi();
            WireGlobalKeys();
            RefreshStatus();
        });

        Application.Run();
    }

    private void ShowSplash(Action onDone)
    {
        var splash = new Window()
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            Border = new Border() { BorderStyle = BorderStyle.None }
        };

        var art = new StringBuilder();
        art.AppendLine("   __  ___                 ");
        art.AppendLine("  /  |/  /___ _____ ___  __");
        art.AppendLine(" / /|_/ / __ `/ __ `__ \\/ /");
        art.AppendLine("/ /  / / /_/ / / / / / / / ");
        art.AppendLine("/_/  /_/\\__,_/_/ /_/ /_/_/  ");
        art.AppendLine("");
        art.AppendLine("MagusTui v0.2 — Terminal Workspace");
        art.AppendLine("F2 Mode • F3 Backend • F4 Theme • Ctrl+P Palette");
        art.AppendLine("");

        var label = new Label(art.ToString())
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            TextAlignment = TextAlignment.Centered,
        };

        splash.Add(label);
        Application.Top.Add(splash);

        // Apply first theme before showing UI
        _themes.LoadAll();
        _themes.ApplyCurrent();

        // 1.1s timeout then swap to main UI
        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(1100), _ =>
        {
            Application.Top.Remove(splash);
            onDone();
            return false;
        });
    }

    private void BuildUi()
    {
        _main = new Window("Magus — Terminal Workspace")
        {
            X = 0, Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1
        };

        var menu = MenuFactory.Build(
            themes: _themes,
            backend: _backend,
            modes: _modes,
            onThemeChanged: () => { _themes.ApplyCurrent(); RefreshStatus(); Log($"Theme: {_themes.CurrentName}"); },
            onBackendChanged: () => { RefreshStatus(); Log($"Backend: {_backend.CurrentName}"); },
            onModeChanged: () => { RefreshStatus(); Log($"Mode: {_modes.CurrentName}"); },
            onShowPalette: () => ShowPalette(),
            onQuit: () => Application.RequestStop()
        );

        Application.Top.Add(menu);

        // Left navigator + files
        var left = NavigatorPane.Build(
            onCommand: cmd => ExecuteCommand(cmd),
            notesWorkspaceFactory: () => _notes,
            backendFactory: () => _backend,
            modeFactory: () => _modes
        );
        left.X = 0;
        left.Y = 0;
        left.Width = 28;
        left.Height = Dim.Fill();

        // Center area: tabs
        _tabs = new TabView()
        {
            X = Pos.Right(left),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _logs = new LogPane();
        _notes = new NotesWorkspace(_logs);
        _chat = new ChatPane(_logs, _backend, _modes);
        _search = new SearchPane(_logs);

        _chatTab = new TabView.Tab("Chat", _chat.View);
        _searchTab = new TabView.Tab("Search", _search.View);
        _notesTab = new TabView.Tab("Notes", _notes.View);
        _logsTab = new TabView.Tab("Logs", _logs.View);

        _tabs.AddTab(_chatTab, true);
        _tabs.AddTab(_searchTab, false);
        _tabs.AddTab(_notesTab, false);
        _tabs.AddTab(_logsTab, false);

        _main.Add(left, _tabs);

        _status = new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.F1, "~F1~ Help", () => ShowHelp()),
            new StatusItem(Key.F2, "~F2~ Mode", () => { _modes.Cycle(); RefreshStatus(); Log($"Mode: {_modes.CurrentName}"); }),
            new StatusItem(Key.F3, "~F3~ Backend", () => { _backend.Cycle(); RefreshStatus(); Log($"Backend: {_backend.CurrentName}"); }),
            new StatusItem(Key.F4, "~F4~ Theme", () => { _themes.Cycle(); _themes.ApplyCurrent(); RefreshStatus(); Log($"Theme: {_themes.CurrentName}"); }),
            new StatusItem(Key.CtrlMask | Key.P, "~^P~ Palette", () => ShowPalette()),
        });

        Application.Top.Add(_main);
        Application.Top.Add(_status);

        // Register commands for palette
        RegisterCommands();
    }

    private void WireGlobalKeys()
    {
        Application.Top.KeyDown += e =>
        {
            if (e.KeyEvent.Key == (Key.CtrlMask | Key.P))
            {
                e.Handled = true;
                ShowPalette();
            }
        };
    }

    private void RegisterCommands()
    {
        _commands.Add("Switch Tab: Chat", "tabs.chat", () => { if (_chatTab != null) _tabs!.SelectedTab = _chatTab; });
        _commands.Add("Switch Tab: Search", "tabs.search", () => { if (_searchTab != null) _tabs!.SelectedTab = _searchTab; });
        _commands.Add("Switch Tab: Notes", "tabs.notes", () => { if (_notesTab != null) _tabs!.SelectedTab = _notesTab; });
        _commands.Add("Switch Tab: Logs", "tabs.logs", () => { if (_logsTab != null) _tabs!.SelectedTab = _logsTab; });

        _commands.Add("Theme: Next", "theme.next", () => { _themes.Cycle(); _themes.ApplyCurrent(); RefreshStatus(); Log($"Theme: {_themes.CurrentName}"); });
        _commands.Add("Backend: Next", "backend.next", () => { _backend.Cycle(); RefreshStatus(); Log($"Backend: {_backend.CurrentName}"); });
        _commands.Add("Mode: Next", "mode.next", () => { _modes.Cycle(); RefreshStatus(); Log($"Mode: {_modes.CurrentName}"); });

        _commands.Add("Notes: Open…", "notes.open", () => { if (_notesTab != null) _tabs!.SelectedTab = _notesTab; _notes!.OpenDialog(); });
        _commands.Add("Notes: Save", "notes.save", () => { if (_notesTab != null) _tabs!.SelectedTab = _notesTab; _notes!.Save(); });
        _commands.Add("Notes: Save As…", "notes.saveas", () => { if (_notesTab != null) _tabs!.SelectedTab = _notesTab; _notes!.SaveAsDialog(); });

        _commands.Add("Search: Run demo query", "search.demo", () => { if (_searchTab != null) _tabs!.SelectedTab = _searchTab; _search!.RunDemo(); });

        _commands.Add("Help", "help", () => ShowHelp());
        _commands.Add("Quit", "quit", () => Application.RequestStop());
    }

    private void ShowPalette()
    {
        var dlg = CommandPalette.Create(_commands, execute: id => ExecuteCommand(id));
        Application.Run(dlg);
    }

    private void ExecuteCommand(string id)
    {
        if (_commands.TryExecute(id))
        {
            Log($"Command: {id}");
            RefreshStatus();
        }
        else
        {
            Log($"Unknown command: {id}");
        }
    }

    private void ShowHelp()
    {
        var okButton = new Button("OK", is_default: true);
        okButton.Clicked += () => Application.RequestStop();

        var help = new Dialog("Help", 72, 18, okButton)
        {
            Modal = true
        };

        var text =
            "MagusTui v0.2\n\n" +
            "Keys:\n" +
            "  F2  Cycle Mode (Explore/Build/Reflect)\n" +
            "  F3  Cycle Backend\n" +
            "  F4  Cycle Theme\n" +
            "  Ctrl+P  Command Palette\n\n" +
            "Tabs:\n" +
            "  Chat • Search • Notes • Logs\n\n" +
            "Notes:\n" +
            "  Uses a sandbox folder under your user profile.\n";

        help.Add(new TextView()
        {
            Text = text,
            ReadOnly = true,
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 4
        });

        Application.Run(help);
    }

    private void RefreshStatus()
    {
        if (_status is null) return;

        _status.Items = new StatusItem[]
        {
            new StatusItem(Key.F1, "~F1~ Help", () => ShowHelp()),
            new StatusItem(Key.F2, $"~F2~ Mode: {_modes.CurrentName}", () => { _modes.Cycle(); RefreshStatus(); Log($"Mode: {_modes.CurrentName}"); }),
            new StatusItem(Key.F3, $"~F3~ Backend: {_backend.CurrentName}", () => { _backend.Cycle(); RefreshStatus(); Log($"Backend: {_backend.CurrentName}"); }),
            new StatusItem(Key.F4, $"~F4~ Theme: {_themes.CurrentName}", () => { _themes.Cycle(); _themes.ApplyCurrent(); RefreshStatus(); Log($"Theme: {_themes.CurrentName}"); }),
            new StatusItem(Key.CtrlMask | Key.P, "~^P~ Palette", () => ShowPalette()),
        };
    }

    private void Log(string message) => _logs?.Add(message);
}
