using System.Text.Json;
using System.Text;

namespace Harmonizer.Services.LLM;

// Simple stub that echoes back the input JSON. Replace with real LLM integration (OpenAI, Azure, etc.)
public class StubLLMClient : ILLMClient
{
    public Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Extract last JSON block heuristically (demo purposes only)
        var startIdx = prompt.LastIndexOf('{');
        var endIdx = prompt.LastIndexOf('}');
        if (startIdx >= 0 && endIdx > startIdx)
        {
            var json = prompt.Substring(startIdx, endIdx - startIdx + 1);
            // Return pretty printed
            try
            {
                var doc = JsonDocument.Parse(json);
                var pretty = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                return Task.FromResult(pretty);
            }
            catch
            {
                // fall through
            }
        }
        // Fallback: return minimal JSON
        return Task.FromResult("{ \"echo\": \"harmonization stub output\" }");
    }
}
