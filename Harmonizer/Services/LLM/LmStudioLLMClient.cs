using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Harmonizer.Services.LLM;

public class LlmOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 1024;
}

public class LmStudioLlmClient : ILLMClient // renamed
{
    private readonly HttpClient _http;
    private readonly LlmOptions _options;
    public LmStudioLlmClient(HttpClient http, IOptions<LlmOptions> options)
    {
        _http = http;
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            _http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
        }
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Assume LM Studio provides OpenAI-compatible /v1/chat/completions endpoint
        var request = new
        {
            model = _options.Model,
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens,
            messages = new[]
            {
                new { role = "system", content = "You are a JSON harmonization assistant. Output ONLY valid JSON." },
                new { role = "user", content = prompt }
            }
        };

        var response = await _http.PostAsJsonAsync("/v1/chat/completions", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Serialize(new { error = "LLM_HTTP_ERROR", status = (int)response.StatusCode, body = errorText });
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        // Try to extract first choice content
        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
        {
            var first = choices[0];
            if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
            {
                return content.GetString() ?? "{}";
            }
        }
        return JsonSerializer.Serialize(new { error = "LLM_UNEXPECTED_RESPONSE", raw = doc.RootElement });
    }
}
