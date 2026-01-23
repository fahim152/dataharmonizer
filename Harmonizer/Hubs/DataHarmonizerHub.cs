using Microsoft.AspNetCore.SignalR;

namespace Harmonizer.Hubs;

public class DataHarmonizerHub : Hub
{
    private readonly ILogger<DataHarmonizerHub> _logger;

    public DataHarmonizerHub(ILogger<DataHarmonizerHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("🔗 SignalR client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("❌ SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinWorkflowGroup(int workflowId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
        _logger.LogInformation("✅ Client {ConnectionId} joined workflow_{WorkflowId} group", Context.ConnectionId, workflowId);
    }

    public async Task LeaveWorkflowGroup(int workflowId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workflow_{workflowId}");
        _logger.LogInformation("👋 Client {ConnectionId} left workflow_{WorkflowId} group", Context.ConnectionId, workflowId);
    }
}
