
using Terminal.Gui;

namespace MagusTui;

public sealed class LogPane
{
    private readonly List<string> _lines = new();
    private readonly TextView _view;

    public View View { get; }

    public LogPane()
    {
        _view = new TextView()
        {
            ReadOnly = true,
            WordWrap = false
        };

        View = _view;
        Add("MagusTui: ready.");
    }

    public void Add(string message)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss");
        _lines.Add($"[{ts}] {message}");

        if (_lines.Count > 500)
            _lines.RemoveRange(0, _lines.Count - 500);

        _view.Text = string.Join(Environment.NewLine, _lines);
        _view.MoveEnd();
    }
}
