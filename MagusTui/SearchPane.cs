
using Terminal.Gui;

namespace MagusTui;

public sealed class SearchPane
{
    private readonly LogPane _logs;
    private readonly TextField _query;
    private readonly ListView _results;

    public View View { get; }

    public SearchPane(LogPane logs)
    {
        _logs = logs;

        var root = new View() { Width = Dim.Fill(), Height = Dim.Fill() };

        var lbl = new Label("Search")
        {
            X = 0, Y = 0
        };

        _query = new TextField("")
        {
            X = 0, Y = 1,
            Width = Dim.Fill()
        };

        var btn = new Button("Run")
        {
            X = Pos.Right(_query) - 8,
            Y = 2
        };
        btn.Clicked += () => Run();

        _results = new ListView(new List<string>())
        {
            X = 0, Y = 3,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _query.KeyDown += e =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                e.Handled = true;
                Run();
            }
        };

        root.Add(lbl, _query, btn, _results);
        View = root;

        _results.SetSource(new List<string> {
            "Magus Search (demo): type a query and press Enter.",
            "Tip: wire this to your local index + embeddings next."
        });
    }

    public void RunDemo()
    {
        _query.Text = "stonehenge magical";
        Run();
    }

    private void Run()
    {
        var q = _query.Text?.ToString() ?? "";
        q = q.Trim();
        if (q.Length == 0) return;

        var items = new List<string>
        {
            $"Result 1: {q} — (demo snippet)",
            $"Result 2: {q} — (demo snippet)",
            $"Result 3: {q} — (demo snippet)",
        };

        _results.SetSource(items);
        _logs.Add($"Search: {q}");
    }
}
