import * as signalR from '@microsoft/signalr';

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5144';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Map<string, Set<Function>> = new Map();

  async connect() {
    if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE}/harmonizerhub`)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('HarmonizationComplete', (data) => {
      console.log('📡 SignalR: HarmonizationComplete received', data);
      this.notifyListeners('HarmonizationComplete', data);
    });

    this.connection.on('HarmonizationError', (data) => {
      console.log('📡 SignalR: HarmonizationError received', data);
      this.notifyListeners('HarmonizationError', data);
    });

    this.connection.onreconnecting(() => {
      console.log('SignalR Reconnecting...');
    });

    this.connection.onreconnected(() => {
      console.log('SignalR Reconnected!');
    });

    this.connection.onclose(() => {
      console.log('SignalR Disconnected');
    });

    try {
      await this.connection.start();
      console.log('✅ SignalR Connected Successfully');
    } catch (err) {
      console.error('❌ SignalR Connection Error:', err);
      setTimeout(() => this.connect(), 5000);
    }
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async joinWorkflowGroup(workflowId: number) {
    // Wait for connection if not connected
    if (this.connection?.state !== signalR.HubConnectionState.Connected) {
      await this.connect();
      // Give it a moment to fully establish
      await new Promise(resolve => setTimeout(resolve, 100));
    }
    
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('JoinWorkflowGroup', workflowId);
        console.log(`✅ Joined workflow group ${workflowId}`);
      } catch (err) {
        console.error(`Failed to join workflow group ${workflowId}:`, err);
      }
    }
  }

  async leaveWorkflowGroup(workflowId: number) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke('LeaveWorkflowGroup', workflowId);
        console.log(`Left workflow group ${workflowId}`);
      } catch (err) {
        console.error(`Failed to leave workflow group ${workflowId}:`, err);
      }
    }
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  on(event: string, callback: Function) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: Function) {
    if (this.listeners.has(event)) {
      this.listeners.get(event)!.delete(callback);
    }
  }

  private notifyListeners(event: string, data: any) {
    if (this.listeners.has(event)) {
      this.listeners.get(event)!.forEach(callback => callback(data));
    }
  }
}

export const signalRService = new SignalRService();
