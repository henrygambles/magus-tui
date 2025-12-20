
using Terminal.Gui;

namespace MagusTui;

public static class NavigatorPane
{
    public static View Build(
        Action<string> onCommand,
        Func<NotesWorkspace?> notesWorkspaceFactory,
        Func<BackendManager?> backendFactory,
        Func<ModeManager?> modeFactory)
    {
        var root = new FrameView("Navigator")
        {
            Width = 28,
            Height = Dim.Fill()
        };

        var cmds = new List<(string Title, string Id)>
        {
            ("Search →", "tabs.search"),
            ("Ask →", "tabs.chat"),
            ("Open Notes →", "tabs.notes"),
            ("Export Report →", "todo.export"),
            ("Toggle Mode →", "mode.next"),
            ("Backend Next →", "backend.next"),
            ("Theme Next →", "theme.next"),
        };

        var cmdList = new ListView(cmds.Select(c => c.Title).ToList())
        {
            X = 1, Y = 1,
            Width = Dim.Fill() - 2,
            Height = 9
        };
        cmdList.OpenSelectedItem += _ =>
        {
            var id = cmds[Math.Clamp(cmdList.SelectedItem, 0, cmds.Count - 1)].Id;
            onCommand(id);
        };

        var files = new FrameView("Files")
        {
            X = 0,
            Y = Pos.Bottom(cmdList) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var fileList = new ListView(new List<string>())
        {
            X = 1, Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2
        };

        void RefreshFiles()
        {
            var ws = notesWorkspaceFactory();
            var list = ws?.ListFiles() ?? new List<string>() { "(no sandbox yet)" };
            fileList.SetSource(list);
        }

        fileList.OpenSelectedItem += _ =>
        {
            var ws = notesWorkspaceFactory();
            if (ws is null) return;

            var idx = fileList.SelectedItem;
            var name = ws.ListFiles();
            if (idx >= 0 && idx < name.Count)
                ws.Open(name[idx]);
        };

        files.Add(fileList);
        root.Add(cmdList, files);

        Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(250), _ =>
        {
            RefreshFiles();
            return true;
        });

        return root;
    }
}
