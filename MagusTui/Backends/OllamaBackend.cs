using System.Net.Http.Json;
using System.Text.Json;
using MagusTui.Models;
using System.Net;

namespace MagusTui.Backends;

public class OllamaBackend : IChatBackend
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _baseUrl;
    private bool _probed;
    private readonly bool _baseAlreadyHasApi;

    public string Name => "Local (Ollama)";

    public OllamaBackend(HttpClient http, string baseUrl, string model)
    {
        _http = http;
        _baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
        _baseAlreadyHasApi = _baseUrl.TrimEnd('/').EndsWith("/api", StringComparison.OrdinalIgnoreCase);
        _http.BaseAddress = new Uri(_baseUrl);
        _model = model;
    }

    public async Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        await EnsureReady(ct);

        // Use /api/chat when available.
        var payload = new
        {
            model = _model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            stream = false
        };

        HttpResponseMessage? resp = null;
        string? respBody = null;
        try
        {
            resp = await _http.PostAsJsonAsync(PathFor("chat"), payload, ct);
            respBody = await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception)
        {
            // Ignore and fall back to /api/generate
        }

        if (resp != null && resp.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(respBody ?? "");
            if (doc.RootElement.TryGetProperty("message", out var msg) &&
                msg.TryGetProperty("content", out var content))
            {
                return content.GetString() ?? "";
            }
        }

        // Fallback: /api/generate expects a prompt.
        var prompt = string.Join("\n\n", messages.Select(m => $"{m.Role}: {m.Content}"));
        var genPayload = new { model = _model, prompt, stream = false };
        using var genResp = await _http.PostAsJsonAsync(PathFor("generate"), genPayload, ct);
        var genBody = await genResp.Content.ReadAsStringAsync(ct);
        if (!genResp.IsSuccessStatusCode)
        {
            var hint = genResp.StatusCode == HttpStatusCode.NotFound
                ? " (model name may be missing locally)"
                : "";
            throw new Exception($"Ollama error ({(int)genResp.StatusCode}): {genBody}{hint}");
        }
        using var genDoc = JsonDocument.Parse(genBody);
        if (genDoc.RootElement.TryGetProperty("response", out var r))
            return r.GetString() ?? "";

        return "";
    }

    private async Task EnsureReady(CancellationToken ct)
    {
        if (_probed) return;
        _probed = true;

        HttpResponseMessage? resp = null;
        string? body = null;
        try
        {
            resp = await _http.GetAsync(PathFor("tags"), ct);
            body = await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not reach Ollama at {_baseUrl}: {ex.Message}. Check OllamaBaseUrl.", ex);
        }

        if (resp is null || !resp.IsSuccessStatusCode)
        {
            var snippet = string.IsNullOrWhiteSpace(body) ? resp?.ReasonPhrase : body;
            throw new Exception($"Ollama at {_baseUrl} returned {(int)(resp?.StatusCode ?? 0)} on /api/tags: {snippet}. Ensure Ollama is running and that OllamaBaseUrl points to it (e.g., http://127.0.0.1:11434).");
        }

        try
        {
            using var doc = JsonDocument.Parse(body ?? "");
            var names = doc.RootElement
                .GetProperty("models")
                .EnumerateArray()
                .Select(m => m.TryGetProperty("name", out var n) ? n.GetString() : null)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .ToList();

            if (!names.Any(n => string.Equals(n, _model, StringComparison.OrdinalIgnoreCase)))
            {
                var suggestion = GuessModelName(names);
                var list = names.Count == 0 ? "(no models listed; pull one with `ollama pull <model>`)" : string.Join(", ", names);
                var hint = suggestion is null ? "" : $" Did you mean '{suggestion}'?";
                throw new Exception($"Model '{_model}' not found on Ollama. Available: {list}.{hint}");
            }
        }
        catch (Exception ex) when (ex is JsonException)
        {
            throw new Exception($"Unexpected response from Ollama at {_baseUrl} when listing models: {ex.Message}", ex);
        }
    }

    private string? GuessModelName(List<string> names)
    {
        // Common typo: qwen.2.5 vs qwen2.5
        if (_model.Contains("qwen.2.5", StringComparison.OrdinalIgnoreCase))
        {
            var candidate = _model.Replace("qwen.2.5", "qwen2.5", StringComparison.OrdinalIgnoreCase);
            if (names.Any(n => string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase)))
                return candidate;
        }
        return null;
    }

    private string PathFor(string endpoint)
    {
        endpoint = endpoint.TrimStart('/');
        return _baseAlreadyHasApi ? endpoint : $"api/{endpoint}";
    }
}
