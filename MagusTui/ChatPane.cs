
using Terminal.Gui;

namespace MagusTui;

public sealed class ChatPane
{
    private readonly LogPane _logs;
    private readonly BackendManager _backend;
    private readonly ModeManager _modes;

    private readonly TextView _history;
    private readonly TextField _input;

    public View View { get; }

    public ChatPane(LogPane logs, BackendManager backend, ModeManager modes)
    {
        _logs = logs;
        _backend = backend;
        _modes = modes;

        var root = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _history = new TextView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2,
            ReadOnly = true,
            WordWrap = true
        };

        _input = new TextField("")
        {
            X = 0, Y = Pos.Bottom(_history),
            Width = Dim.Fill(),
            Height = 1
        };

        var hint = new Label("Enter to send • F2 mode • F3 backend • F4 theme • Ctrl+P palette")
        {
            X = 0, Y = Pos.Bottom(_input),
            Width = Dim.Fill()
        };

        _input.KeyDown += e =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                e.Handled = true;
                Send();
            }
        };

        root.Add(_history, _input, hint);
        View = root;

        Append("Tara", "Welcome to Magus TUI.");
        Append("Tip", "Use F2 to switch modes (Explore/Build/Reflect).");
    }

    private void Send()
    {
        var text = _input.Text?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(text)) return;

        Append("You", text.Trim());

        // Offline demo response
        var response = _backend.CurrentName.Contains("Offline", StringComparison.OrdinalIgnoreCase)
            ? $"(offline) I received that in {_modes.CurrentName} mode."
            : $"({ _backend.CurrentName.ToLowerInvariant() }) I received that in {_modes.CurrentName} mode.";

        Append("Tara", response);

        _logs.Add($"Chat: {text.Trim()}");
        _input.Text = "";
    }

    private void Append(string who, string text)
    {
        var current = _history.Text?.ToString() ?? "";
        var next = string.IsNullOrWhiteSpace(current)
            ? $"{who}: {text}"
            : $"{current}{Environment.NewLine}{Environment.NewLine}{who}: {text}";
        _history.Text = next;
        _history.MoveEnd();
    }
}
