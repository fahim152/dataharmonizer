using System.Text;
using System.Text.Json;
using Harmonizer.Data;
using Harmonizer.Models;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.Services;

public interface IOutboundApiService
{
    Task<bool> SendRecordAsync(HarmonizedRecord record, Workflow workflow);
    Task SendUnsentRecordsAsync(int workflowId);
}

public class OutboundApiService : IOutboundApiService
{
    private readonly HarmonizerDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OutboundApiService> _logger;

    public OutboundApiService(
        HarmonizerDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<OutboundApiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> SendRecordAsync(HarmonizedRecord record, Workflow workflow)
    {
        if (!workflow.OutboundApiEnabled || string.IsNullOrEmpty(workflow.OutboundApiUrl))
        {
            return false;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Add custom headers if configured
            if (!string.IsNullOrEmpty(workflow.OutboundApiHeaders))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(workflow.OutboundApiHeaders);
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse outbound API headers for workflow {WorkflowId}", workflow.Id);
                }
            }

            // Send harmonized JSON
            var content = new StringContent(
                record.HarmonizedJson ?? "{}",
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(workflow.OutboundApiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                // Mark as sent
                record.SentToOutboundApi = true;
                record.SentToOutboundApiAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Successfully sent record {RecordId} to outbound API {Url} for workflow {WorkflowId}",
                    record.Id,
                    workflow.OutboundApiUrl,
                    workflow.Id
                );
                
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send record {RecordId} to outbound API. Status: {StatusCode}",
                    record.Id,
                    response.StatusCode
                );
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending record {RecordId} to outbound API for workflow {WorkflowId}", 
                record.Id, workflow.Id);
            return false;
        }
    }

    public async Task SendUnsentRecordsAsync(int workflowId)
    {
        try
        {
            var workflow = await _context.Workflows
                .FirstOrDefaultAsync(w => w.Id == workflowId);

            if (workflow == null || !workflow.OutboundApiEnabled)
            {
                return;
            }

            // Get all unsent records for this workflow
            var requests = await _context.HarmonizeRequests
                .Where(r => r.WorkflowId == workflowId)
                .ToListAsync();

            var requestIds = requests.Select(r => r.Id).ToList();

            var unsentRecords = await _context.HarmonizedRecords
                .Where(r => requestIds.Contains(r.HarmonizeRequestId) && !r.SentToOutboundApi)
                .OrderBy(r => r.ReceivedAt)
                .ToListAsync();

            _logger.LogInformation(
                "Found {Count} unsent records for workflow {WorkflowId}. Starting batch send...",
                unsentRecords.Count,
                workflowId
            );

            int successCount = 0;
            int failCount = 0;

            foreach (var record in unsentRecords)
            {
                var success = await SendRecordAsync(record, workflow);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }

                // Small delay to avoid overwhelming the target API
                await Task.Delay(100);
            }

            _logger.LogInformation(
                "Completed batch send for workflow {WorkflowId}. Success: {Success}, Failed: {Failed}",
                workflowId,
                successCount,
                failCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending unsent records for workflow {WorkflowId}", workflowId);
        }
    }
}
