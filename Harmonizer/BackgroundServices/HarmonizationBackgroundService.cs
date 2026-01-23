using System.Threading.Channels;
using Harmonizer.Models;
using Harmonizer.Services;
using Harmonizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Harmonizer.BackgroundServices;

public class HarmonizationJob
{
    public required Workflow Workflow { get; set; }
    public required HarmonizeRequest Request { get; set; }
}

public class HarmonizationBackgroundService : BackgroundService
{
    private readonly Channel<HarmonizationJob> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HarmonizationBackgroundService> _logger;

    public HarmonizationBackgroundService(
        Channel<HarmonizationJob> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<HarmonizationBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Harmonization Background Service started");

        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessHarmonizationAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing harmonization job for request {RequestId}", job.Request.Id);
            }
        }
    }

    private async Task ProcessHarmonizationAsync(HarmonizationJob job, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var recordService = scope.ServiceProvider.GetRequiredService<IHarmonizedRecordService>();
        var dynamicTableService = scope.ServiceProvider.GetRequiredService<IDynamicTableService>();
        var outboundApiService = scope.ServiceProvider.GetRequiredService<IOutboundApiService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DataHarmonizerHub>>();

        try
        {
            // Perform harmonization
            var harmonizedRecord = await recordService.HarmonizeAsync(
                job.Request.Id,
                job.Workflow.ContextPrompt,
                job.Workflow.DesiredOutput);

            // Insert into dynamic table if workflow has team and entity
            if (job.Workflow.TeamId != null && !string.IsNullOrEmpty(job.Workflow.EntityName))
            {
                await dynamicTableService.InsertHarmonizedDataAsync(job.Workflow, harmonizedRecord.HarmonizedJson ?? "{}");
            }

            // Send to outbound API if enabled
            if (job.Workflow.OutboundApiEnabled && !string.IsNullOrEmpty(job.Workflow.OutboundApiUrl))
            {
                // Get the actual record entity to send
                var recordEntity = await scope.ServiceProvider.GetRequiredService<Data.HarmonizerDbContext>()
                    .HarmonizedRecords
                    .FirstOrDefaultAsync(r => r.Id == harmonizedRecord.Id, cancellationToken);
                
                if (recordEntity != null)
                {
                    await outboundApiService.SendRecordAsync(recordEntity, job.Workflow);
                }
            }

            // Send SignalR notification
            _logger.LogInformation("📡 Sending SignalR notification to workflow_{WorkflowId} group", job.Workflow.Id);
            
            await hubContext.Clients.Group($"workflow_{job.Workflow.Id}")
                .SendAsync("HarmonizationComplete", new
                {
                    workflowId = job.Workflow.Id,
                    requestId = job.Request.Id,
                    recordId = harmonizedRecord.Id,
                    timestamp = DateTime.UtcNow
                }, cancellationToken);

            _logger.LogInformation("✅ Harmonization completed and SignalR notification sent for request {RequestId}", job.Request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to harmonize request {RequestId}", job.Request.Id);
            
            // Send error notification
            await hubContext.Clients.Group($"workflow_{job.Workflow.Id}")
                .SendAsync("HarmonizationError", new
                {
                    workflowId = job.Workflow.Id,
                    requestId = job.Request.Id,
                    error = ex.Message
                }, cancellationToken);
        }
    }
}
