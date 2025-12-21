using MagusTui.Backends;
using MagusTui.Config;
using MagusTui.Models;

namespace MagusTui;

public class BackendManager
{
    private readonly List<IChatBackend> _backends = new();
    private int _index;

    public BackendManager(MagusConfig config)
    {
        // Each backend gets its own HttpClient because they need different base addresses/headers.
        _backends.Add(new OllamaBackend(new HttpClient(), config.OllamaBaseUrl, config.OllamaModel));
        _backends.Add(new OpenAICompatibleBackend(new HttpClient(), config.LmStudioBaseUrl, config.LmStudioModel, name: "Local (LM Studio)", apiKey: null));

        var cloudApiKey = string.IsNullOrWhiteSpace(config.CloudApiKeyEnv)
            ? null
            : Environment.GetEnvironmentVariable(config.CloudApiKeyEnv);
        _backends.Add(new OpenAICompatibleBackend(new HttpClient(), config.CloudBaseUrl, config.CloudModel, name: "Cloud (API)", apiKey: cloudApiKey));
        _backends.Add(new OfflineBackend());

        _index = 0;
    }

    public IChatBackend Current => _backends[_index];
    public string CurrentName => Current.Name;
    public IReadOnlyList<string> Names => _backends.Select(b => b.Name).ToList();

    public void Next()
    {
        _index = (_index + 1) % _backends.Count;
    }

    public void Cycle() => Next();

    public IReadOnlyList<IChatBackend> All => _backends;

    public Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct) =>
        Current.ChatAsync(messages, ct);

    public void Set(string name) => SelectByName(name);

    public void SelectByName(string name)
    {
        var idx = _backends.FindIndex(b => b.Name == name);
        if (idx >= 0) _index = idx;
    }
}
