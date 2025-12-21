using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MagusTui.Models;

namespace MagusTui.Backends;

// Works with OpenAI API and OpenAI-compatible servers (e.g., LM Studio).
public class OpenAICompatibleBackend : IChatBackend
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _name;

    public string Name => _name;

    public OpenAICompatibleBackend(HttpClient http, string baseUrl, string model, string name, string? apiKey)
    {
        _http = http;
        _http.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
        if (!string.IsNullOrWhiteSpace(apiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _model = model;
        _name = name;
    }

    public async Task<string> ChatAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        var payload = new
        {
            model = _model,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = 0.7,
            stream = false
        };

        using var resp = await _http.PostAsJsonAsync("v1/chat/completions", payload, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"OpenAI-compatible error ({(int)resp.StatusCode}): {json}");

        using var doc = JsonDocument.Parse(json);
        // choices[0].message.content
        var root = doc.RootElement;
        var content = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? "";
    }
}
