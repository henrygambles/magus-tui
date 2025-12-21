using System.Text.Json;

namespace MagusTui.Config;

public class MagusConfig
{
    public string PersonaFile { get; set; } = "personas/persona1.md";
    public int MemoryMaxMessages { get; set; } = 24;
    public string MemoryFile { get; set; } = "memory/conversation.jsonl";

    // Backend preferences
    public string OllamaBaseUrl { get; set; } = "http://127.0.0.1:11434";
    public string OllamaModel { get; set; } = "qwen2.5:7b";

    public string LmStudioBaseUrl { get; set; } = "http://127.0.0.1:1234/v1";
    public string LmStudioModel { get; set; } = "llama-3.1-8b-instruct";
    public string CloudBaseUrl { get; set; } = "https://api.openai.com/v1";
    public string? CloudApiKeyEnv { get; set; } = "OPENAI_API_KEY";
    public string CloudModel { get; set; } = "gpt-4o-mini";

    public static MagusConfig LoadOrCreate() => LoadOrCreateDefault();

    public static MagusConfig LoadOrCreateDefault()
    {
        MagusHome.Ensure();
        if (!File.Exists(MagusHome.ConfigPath))
        {
            var cfg = new MagusConfig();
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(MagusHome.ConfigPath, json);
            return cfg;
        }

        try
        {
            var txt = File.ReadAllText(MagusHome.ConfigPath);
            return JsonSerializer.Deserialize<MagusConfig>(txt) ?? new MagusConfig();
        }
        catch
        {
            return new MagusConfig();
        }
    }
}
