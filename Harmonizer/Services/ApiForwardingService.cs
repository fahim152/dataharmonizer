using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Harmonizer.Models;

namespace Harmonizer.Services;

public interface IApiForwardingService
{
    /// <summary>
    /// Forwards harmonized data to the configured API endpoint
    /// </summary>
    Task<ApiForwardResult> ForwardToApiAsync(OutputConfig config, string harmonizedJson);
}

public class ApiForwardResult
{
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}

public class ApiForwardingService : IApiForwardingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiForwardingService> _logger;

    public ApiForwardingService(HttpClient httpClient, ILogger<ApiForwardingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiForwardResult> ForwardToApiAsync(OutputConfig config, string harmonizedJson)
    {
        if (string.IsNullOrEmpty(config.ApiEndpointUrl))
        {
            return new ApiForwardResult
            {
                Success = false,
                Error = "API endpoint URL not configured"
            };
        }

        try
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(config.ApiEndpointUrl),
                Method = GetHttpMethod(config.ApiHttpMethod),
                Content = new StringContent(harmonizedJson, Encoding.UTF8, "application/json")
            };

            // Add custom headers
            if (!string.IsNullOrEmpty(config.ApiHeaders))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(config.ApiHeaders);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse custom headers");
                }
            }

            // Add authentication
            ApplyAuthentication(request, config);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "API forward to {Url}: Status {StatusCode}", 
                config.ApiEndpointUrl, 
                (int)response.StatusCode);

            return new ApiForwardResult
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Response = responseContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forward to API: {Url}", config.ApiEndpointUrl);
            return new ApiForwardResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private void ApplyAuthentication(HttpRequestMessage request, OutputConfig config)
    {
        if (string.IsNullOrEmpty(config.ApiAuthCredentials)) return;

        switch (config.ApiAuthType)
        {
            case AuthType.ApiKey:
                // Expects format: "HeaderName:Value" or just "Value" (uses X-API-Key)
                var parts = config.ApiAuthCredentials.Split(':', 2);
                if (parts.Length == 2)
                {
                    request.Headers.TryAddWithoutValidation(parts[0], parts[1]);
                }
                else
                {
                    request.Headers.TryAddWithoutValidation("X-API-Key", config.ApiAuthCredentials);
                }
                break;

            case AuthType.Bearer:
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiAuthCredentials);
                break;

            case AuthType.Basic:
                // Expects format: "username:password"
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ApiAuthCredentials));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                break;
        }
    }

    private static HttpMethod GetHttpMethod(string? method)
    {
        return method?.ToUpperInvariant() switch
        {
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            _ => HttpMethod.Post
        };
    }
}
