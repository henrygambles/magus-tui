using Terminal.Gui;
using MagusTui.Config;
using MagusTui.Models;

namespace MagusTui;

public sealed class ChatPane
{
    private readonly LogPane _logs;
    private readonly BackendManager _backend;
    private readonly ModeManager _modes;
    private readonly MemoryStore _memory;
    private readonly MagusConfig _config;
    private string _persona;
    private string _personaName;

    private readonly List<ChatMessage> _conversation = new(); // excludes system

    private readonly TextView _output;
    private readonly TextField _input;
    private readonly Button _send;

    public View Root { get; }
    public View View => Root;
    public void FocusInput() => _input.SetFocus();

    public ChatPane(LogPane logs, BackendManager backend, ModeManager modes, MemoryStore memory, MagusConfig config, string personaName, string personaText)
    {
        _logs = logs;
        _backend = backend;
        _modes = modes;
        _memory = memory;
        _config = config;
        _personaName = personaName;
        _persona = personaText;

        Root = new FrameView("Chat") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

        _output = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 3,
            ReadOnly = true,
            WordWrap = true
        };

        _input = new TextField
        {
            X = 0,
            Y = Pos.Bottom(_output) + 1,
            Width = Dim.Fill() - 12,
            Height = 1
        };

        _send = new Button("Send")
        {
            X = Pos.Right(_input) + 1,
            Y = Pos.Top(_input),
            Width = 10,
            Height = 1,
            IsDefault = true
        };

        _send.Clicked += () => _ = SendAsync();

        _input.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                args.Handled = true;
                _ = SendAsync();
            }
        };

        Root.Add(_output, _input, _send);

        // Load memory and greet
        foreach (var m in _memory.LoadRecent(_config.MemoryMaxMessages))
            _conversation.Add(m);

        if (_conversation.Count == 0)
        {
            Append("Tara", "Welcome to Magus TUI.\nTip: Use F2 to switch modes (Explore/Build/Reflect).\n");
        }
        else
        {
            foreach (var m in _conversation)
                Append(m.Role == "assistant" ? "Tara" : "You", m.Content);
        }
    }

    private async Task SendAsync()
    {
        var text = (_input.Text?.ToString() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        _input.Text = "";
        Append("You", text);

        var userMsg = new ChatMessage("user", text);
        _conversation.Add(userMsg);
        _memory.Append(userMsg);

        _logs.Add($"Chat: {text}");
        _logs.Add($"Mode: {_modes.Current}");
        _logs.Add($"Backend: {_backend.CurrentName}");

        // Build prompt
        var window = _config.MemoryMaxMessages;
        var recent = _conversation.TakeLast(Math.Max(1, window)).ToList();
        var messages = new List<ChatMessage>
        {
            new("system", _persona + "\n\nCurrent mode: " + _modes.Current)
        };
        messages.AddRange(recent);

        try
        {
            SetBusy(true);
            var reply = await _backend.ChatAsync(messages, CancellationToken.None);
            var assistantMsg = new ChatMessage("assistant", reply);
            _conversation.Add(assistantMsg);
            _memory.Append(assistantMsg);
            Append("Tara", reply);
        }
        catch (Exception ex)
        {
            Append("Tara", $"Hmm, something went wrong talking to {_backend.CurrentName}: {ex.Message}");
            _logs.Add($"Error: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Append(string who, string text)
    {
        var line = $"{who}: {text}";
        if (!line.EndsWith("\n")) line += "\n";
        _output.Text = (_output.Text?.ToString() ?? "") + line;
        _output.MoveEnd();
    }

    private void SetBusy(bool busy)
    {
        _send.Enabled = !busy;
        _input.Enabled = !busy;
    }

    public void SetPersona(string personaName, string personaText)
    {
        _personaName = personaName;
        _persona = personaText;
        Append("Tara", $"Persona switched to {_personaName}.");
    }
}
