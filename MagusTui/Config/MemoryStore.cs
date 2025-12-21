using System.Text.Json;
using MagusTui.Models;

namespace MagusTui.Config;

public class MemoryStore
{
    private readonly string _path;

    public MemoryStore(MagusConfig config)
    {
        MagusHome.Ensure();
        _path = Path.Combine(MagusHome.HomeDir, config.MemoryFile);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        if (!File.Exists(_path)) File.WriteAllText(_path, "");
    }

    public List<ChatMessage> LoadRecent(int maxMessages)
    {
        if (!File.Exists(_path)) return new();
        var lines = File.ReadAllLines(_path);
        var take = lines.Reverse().Where(l => !string.IsNullOrWhiteSpace(l)).Take(maxMessages).Reverse();
        var list = new List<ChatMessage>();
        foreach (var line in take)
        {
            try
            {
                var msg = JsonSerializer.Deserialize<ChatMessage>(line);
                if (msg != null) list.Add(msg);
            }
            catch
            {
                // ignore malformed lines
            }
        }
        return list;
    }

    public void Append(ChatMessage msg)
    {
        var json = JsonSerializer.Serialize(msg);
        File.AppendAllText(_path, json + Environment.NewLine);
    }
}
