namespace Harmonizer.Services.LLM;

public interface ILLMClient
{
    Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default);
}
