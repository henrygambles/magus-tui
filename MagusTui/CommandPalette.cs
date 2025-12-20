
using Terminal.Gui;

namespace MagusTui;

public static class CommandPalette
{
    public static Dialog Create(CommandRegistry registry, Action<string> execute)
    {
        var dlg = new Dialog("Command Palette", 72, 20)
        {
            Modal = true
        };

        var filter = new TextField("")
        {
            X = 1, Y = 1,
            Width = Dim.Fill() - 2
        };

        var items = registry.Items.ToList();
        var list = new ListView(items.Select(i => $"{i.Title}   [{i.Id}]").ToList())
        {
            X = 1, Y = 3,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 5
        };

        void Refresh()
        {
            var q = (filter.Text?.ToString() ?? "").Trim().ToLowerInvariant();
            var filtered = string.IsNullOrEmpty(q)
                ? items
                : items.Where(i => i.Title.ToLowerInvariant().Contains(q) || i.Id.ToLowerInvariant().Contains(q)).ToList();

            list.SetSource(filtered.Select(i => $"{i.Title}   [{i.Id}]").ToList());
            list.SelectedItem = 0;
            dlg.SetNeedsDisplay();
        }

        filter.TextChanged += _ => Refresh();

        void RunSelected()
        {
            var q = (filter.Text?.ToString() ?? "").Trim().ToLowerInvariant();
            var filtered = string.IsNullOrEmpty(q)
                ? items
                : items.Where(i => i.Title.ToLowerInvariant().Contains(q) || i.Id.ToLowerInvariant().Contains(q)).ToList();

            if (filtered.Count == 0) return;

            var idx = Math.Clamp(list.SelectedItem, 0, filtered.Count - 1);
            var id = filtered[idx].Id;

            execute(id);
            Application.RequestStop();
        }

        list.OpenSelectedItem += _ => RunSelected();

        dlg.KeyDown += e =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                e.Handled = true;
                RunSelected();
            }
            if (e.KeyEvent.Key == Key.Esc)
            {
                e.Handled = true;
                Application.RequestStop();
            }
        };

        dlg.Add(filter);
        dlg.Add(list);

        var runButton = new Button("Run", is_default: true);
        runButton.Clicked += () => RunSelected();
        var cancelButton = new Button("Cancel");
        cancelButton.Clicked += () => Application.RequestStop();

        dlg.AddButton(runButton);
        dlg.AddButton(cancelButton);

        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(10), _ =>
        {
            filter.SetFocus();
            return false;
        });

        return dlg;
    }
}
