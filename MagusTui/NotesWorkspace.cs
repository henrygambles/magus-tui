
using Terminal.Gui;

namespace MagusTui;

public sealed class NotesWorkspace
{
    private readonly LogPane _logs;
    private readonly TextView _editor;
    private readonly Label _pathLabel;

    private string? _currentPath;

    public View View { get; }

    public NotesWorkspace(LogPane logs)
    {
        _logs = logs;

        var root = new View() { Width = Dim.Fill(), Height = Dim.Fill() };

        var toolbar = new View()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = 2
        };

        var btnOpen = new Button("Open…") { X = 0, Y = 0 };
        btnOpen.Clicked += () => OpenDialog();
        var btnSave = new Button("Save") { X = Pos.Right(btnOpen) + 1, Y = 0 };
        btnSave.Clicked += () => Save();
        var btnSaveAs = new Button("Save As…") { X = Pos.Right(btnSave) + 1, Y = 0 };
        btnSaveAs.Clicked += () => SaveAsDialog();

        _pathLabel = new Label("Sandbox: (none)")
        {
            X = 0, Y = 1,
            Width = Dim.Fill()
        };

        toolbar.Add(btnOpen, btnSave, btnSaveAs, _pathLabel);

        _editor = new TextView()
        {
            X = 0,
            Y = Pos.Bottom(toolbar),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            WordWrap = true
        };

        root.Add(toolbar, _editor);
        View = root;

        EnsureSandbox();
        UpdatePathLabel();
        _editor.Text = "plan.txt\n\n- Rename me\n- Save to sandbox\n- Use the palette (Ctrl+P)\n";
    }

    private string SandboxDir()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dir = Path.Combine(home, ".magus", "sandbox");
        return dir;
    }

    private void EnsureSandbox()
    {
        Directory.CreateDirectory(SandboxDir());
    }

    public List<string> ListFiles()
    {
        EnsureSandbox();
        var files = Directory.GetFiles(SandboxDir())
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OrderBy(n => n)
            .ToList();
        if (files.Count == 0) files.Add("(empty)");
        return files!;
    }

    public void Open(string filename)
    {
        EnsureSandbox();
        if (filename == "(empty)") return;

        var full = Path.Combine(SandboxDir(), filename);
        if (!File.Exists(full)) return;

        _editor.Text = File.ReadAllText(full);
        _currentPath = full;
        UpdatePathLabel();
        _logs.Add($"Notes open: {filename}");
    }

    public void OpenDialog()
    {
        EnsureSandbox();
        var dlg = new OpenDialog("Open Note", "Choose a file in the sandbox")
        {
            CanChooseDirectories = false,
            DirectoryPath = SandboxDir()
        };

        Application.Run(dlg);

        if (!dlg.Canceled && dlg.FilePaths?.Count > 0)
        {
            var path = dlg.FilePaths[0];
            if (File.Exists(path))
            {
                _editor.Text = File.ReadAllText(path);
                _currentPath = path;
                UpdatePathLabel();
                _logs.Add($"Notes open: {Path.GetFileName(path)}");
            }
        }
    }

    public void Save()
    {
        EnsureSandbox();

        if (string.IsNullOrWhiteSpace(_currentPath))
        {
            SaveAsDialog();
            return;
        }

        File.WriteAllText(_currentPath!, _editor.Text?.ToString() ?? "");
        _logs.Add($"Notes saved: {Path.GetFileName(_currentPath)}");
    }

    public void SaveAsDialog()
    {
        EnsureSandbox();

        var dlg = new SaveDialog("Save Note As", "Save into the sandbox")
        {
            DirectoryPath = SandboxDir()
        };

        Application.Run(dlg);

        if (!dlg.Canceled && dlg.FilePath != null)
        {
            var path = dlg.FilePath.ToString();
            if (string.IsNullOrWhiteSpace(path)) return;

            // Ensure within sandbox; if user typed only a name, combine
            if (!Path.IsPathRooted(path))
                path = Path.Combine(SandboxDir(), path);

            File.WriteAllText(path, _editor.Text?.ToString() ?? "");
            _currentPath = path;
            UpdatePathLabel();
            _logs.Add($"Notes saved: {Path.GetFileName(path)}");
        }
    }

    private void UpdatePathLabel()
    {
        var sandbox = SandboxDir();
        var name = string.IsNullOrWhiteSpace(_currentPath) ? "(unsaved)" : Path.GetFileName(_currentPath);
        _pathLabel.Text = $"Sandbox: {sandbox}   |   Current: {name}";
    }
}
