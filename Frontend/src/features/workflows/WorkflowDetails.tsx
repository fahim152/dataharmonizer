import React, { useState, useEffect } from 'react';
import { 
  regenerateWorkflowApiKey, 
  getWorkflowWebhookUrl, 
  fetchRequestsByWorkflow,
  fetchRecords,
  fetchRecordsByWorkflow,
  fetchTenantHierarchy
} from '../../api/harmonizer';
import type { Workflow, HarmonizeRequest, HarmonizedRecord, TenantHierarchy } from '../../types';
import { signalRService } from '../../utils/signalr';

interface WorkflowDetailsProps {
  workflow: Workflow;
  onEdit: () => void;
  onDelete: () => void;
  onRefresh: () => void;
}

const WorkflowDetails: React.FC<WorkflowDetailsProps> = ({ workflow, onEdit, onDelete, onRefresh }) => {
  const [activeTab, setActiveTab] = useState<'overview' | 'pipeline' | 'config' | 'database'>('pipeline');
  const [copying, setCopying] = useState<boolean>(false);
  const [regenerating, setRegenerating] = useState<boolean>(false);
  const [requests, setRequests] = useState<HarmonizeRequest[]>([]);
  const [selectedRequest, setSelectedRequest] = useState<HarmonizeRequest | null>(null);
  const [records, setRecords] = useState<HarmonizedRecord[]>([]);
  const [allWorkflowRecords, setAllWorkflowRecords] = useState<HarmonizedRecord[]>([]);
  const [hierarchy, setHierarchy] = useState<TenantHierarchy[]>([]);
  const [loadingRequests, setLoadingRequests] = useState(false);
  const [showTableModal, setShowTableModal] = useState(false);
  const [tableData, setTableData] = useState<any[]>([]);

  const webhookUrl = getWorkflowWebhookUrl(workflow.apiKey);

  useEffect(() => {
    // Reset state when workflow changes
    setSelectedRequest(null);
    setRequests([]);
    setRecords([]);
    setAllWorkflowRecords([]);
    
    loadRequests();
    loadAllRecords();
    fetchTenantHierarchy().then(setHierarchy).catch(console.error);
    
    // Connect to SignalR and join workflow group
    const setupSignalR = async () => {
      try {
        await signalRService.connect();
        await signalRService.joinWorkflowGroup(workflow.id);
        console.log(`🔗 Joined workflow group: ${workflow.id}`);
      } catch (err) {
        console.error('Failed to setup SignalR:', err);
      }
    };
    
    setupSignalR();

    // Listen for harmonization events
    const handleHarmonizationComplete = (data: any) => {
      console.log('📡 Received HarmonizationComplete event:', data);
      if (data.workflowId === workflow.id) {
        console.log('✅ Reloading data for workflow:', workflow.id);
        loadRequests(); // Reload requests to show new data
        loadAllRecords(); // Reload all records for Database tab
      }
    };

    const handleHarmonizationError = (data: any) => {
      if (data.workflowId === workflow.id) {
        console.error('❌ Harmonization error (real-time):', data);
      }
    };

    signalRService.on('HarmonizationComplete', handleHarmonizationComplete);
    signalRService.on('HarmonizationError', handleHarmonizationError);

    return () => {
      signalRService.off('HarmonizationComplete', handleHarmonizationComplete);
      signalRService.off('HarmonizationError', handleHarmonizationError);
      signalRService.leaveWorkflowGroup(workflow.id);
    };
  }, [workflow.id]);

  const loadRequests = async () => {
    setLoadingRequests(true);
    try {
      const data = await fetchRequestsByWorkflow(workflow.id);
      setRequests(data);
      if (data.length > 0) {
        setSelectedRequest(data[0]);
      } else {
        setSelectedRequest(null); // Clear selection if no requests
      }
    } catch (err) {
      console.error('Failed to load requests:', err);
      setRequests([]);
      setSelectedRequest(null);
    } finally {
      setLoadingRequests(false);
    }
  };

  const loadAllRecords = async () => {
    try {
      const data = await fetchRecordsByWorkflow(workflow.id);
      setAllWorkflowRecords(data);
    } catch (err) {
      console.error('Failed to load workflow records:', err);
    }
  };

  useEffect(() => {
    if (selectedRequest) {
      fetchRecords(selectedRequest.id).then(setRecords).catch(console.error);
    } else {
      setRecords([]);
    }
  }, [selectedRequest]);

  const copyToClipboard = async (text: string): Promise<void> => {
    try {
      await navigator.clipboard.writeText(text);
      setCopying(true);
      setTimeout(() => setCopying(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  };

  const handleRegenerateKey = async (): Promise<void> => {
    if (!window.confirm('Are you sure? This will invalidate the current API key.')) return;
    setRegenerating(true);
    try {
      await regenerateWorkflowApiKey(workflow.id);
      onRefresh();
    } catch (err) {
      console.error('Failed to regenerate key:', err);
    } finally {
      setRegenerating(false);
    }
  };

  const getTeamPath = () => {
    if (!workflow.teamId) return null;
    for (const tenant of hierarchy) {
      for (const ns of tenant.namespaces) {
        for (const team of ns.teams) {
          if (team.id === workflow.teamId) {
            return { tenant: tenant.name, namespace: ns.name, team: team.name };
          }
        }
      }
    }
    return null;
  };

  const teamPath = getTeamPath();
  const generatedTableName = teamPath && workflow.entityName
    ? `${teamPath.tenant.toLowerCase().replace(/\s+/g, '_')}_${teamPath.namespace.toLowerCase()}_${teamPath.team.toLowerCase()}_${workflow.entityName.toLowerCase().replace(/\s+/g, '_')}`
    : null;

  const formatJson = (json: string): string => {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      {/* Header */}
      <div style={{ 
        padding: '16px 24px',
        borderBottom: '1px solid #e0e0e0',
        backgroundColor: '#fff'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h2 style={{ margin: 0, fontSize: 20 }}>{workflow.name}</h2>
            <p style={{ color: '#666', margin: '4px 0 0', fontSize: 13 }}>{workflow.description}</p>
          </div>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <span style={{
              padding: '4px 10px',
              borderRadius: 12,
              backgroundColor: workflow.isActive ? '#e8f5e9' : '#ffebee',
              color: workflow.isActive ? '#2e7d32' : '#c62828',
              fontSize: 12,
              fontWeight: 500
            }}>
              {workflow.isActive ? 'Active' : 'Inactive'}
            </span>
            <button onClick={onEdit} style={{ padding: '6px 14px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
              Edit
            </button>
            <button onClick={onDelete} style={{ padding: '6px 14px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
              Delete
            </button>
          </div>
        </div>

        {/* Tab Navigation */}
        <div style={{ display: 'flex', gap: 4, marginTop: 16 }}>
          {(['pipeline', 'database', 'overview', 'config'] as const).map(tab => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              style={{
                padding: '8px 16px',
                backgroundColor: activeTab === tab ? '#e3f2fd' : 'transparent',
                color: activeTab === tab ? '#1565c0' : '#666',
                border: 'none',
                borderBottom: activeTab === tab ? '2px solid #1565c0' : '2px solid transparent',
                cursor: 'pointer',
                fontSize: 13,
                fontWeight: activeTab === tab ? 600 : 400,
                textTransform: 'capitalize'
              }}
            >
              {tab === 'pipeline' ? 'Data Pipeline' : tab}
            </button>
          ))}
        </div>
      </div>

      {/* Content */}
      <div style={{ flex: 1, overflow: 'auto', backgroundColor: '#f5f5f5' }}>
        {activeTab === 'pipeline' && (
          <div style={{ padding: 24 }}>
            {/* Data Flow Visualization */}
            <div style={{ 
              display: 'grid', 
              gridTemplateColumns: '1fr 40px 1fr 40px 1fr', 
              gap: 0,
              alignItems: 'stretch',
              marginBottom: 24
            }}>
              {/* Inbound */}
              <div style={{ 
                backgroundColor: '#fff', 
                borderRadius: 8, 
                border: '1px solid #e0e0e0',
                overflow: 'hidden'
              }}>
                <div style={{ 
                  padding: '12px 16px', 
                  backgroundColor: '#e3f2fd', 
                  borderBottom: '1px solid #bbdefb',
                  fontWeight: 600,
                  fontSize: 13,
                  color: '#1565c0'
                }}>
                  INBOUND
                </div>
                <div style={{ padding: 16 }}>
                  <div style={{ fontSize: 12, color: '#666', marginBottom: 8 }}>Source Type</div>
                  <div style={{ fontWeight: 500, marginBottom: 16 }}>Webhook / Manual</div>
                  
                  <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Endpoint</div>
                  <code style={{ 
                    display: 'block',
                    fontSize: 11, 
                    backgroundColor: '#f5f5f5', 
                    padding: 8, 
                    borderRadius: 4,
                    wordBreak: 'break-all'
                  }}>
                    {webhookUrl}
                  </code>
                  <button
                    onClick={() => copyToClipboard(webhookUrl)}
                    style={{
                      marginTop: 8,
                      padding: '4px 10px',
                      backgroundColor: copying ? '#4caf50' : '#e0e0e0',
                      color: copying ? '#fff' : '#333',
                      border: 'none',
                      borderRadius: 4,
                      cursor: 'pointer',
                      fontSize: 11
                    }}
                  >
                    {copying ? 'Copied' : 'Copy URL'}
                  </button>
                  
                  <div style={{ marginTop: 12, fontSize: 11, color: '#666', fontStyle: 'italic' }}>
                    Accepts any JSON structure. Data is harmonized based on context prompt.
                  </div>
                </div>
              </div>

              {/* Arrow */}
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <div style={{ fontSize: 20, color: '#999' }}>-&gt;</div>
              </div>

              {/* Transform */}
              <div style={{ 
                backgroundColor: '#fff', 
                borderRadius: 8, 
                border: '1px solid #e0e0e0',
                overflow: 'hidden'
              }}>
                <div style={{ 
                  padding: '12px 16px', 
                  backgroundColor: '#fff3e0', 
                  borderBottom: '1px solid #ffe0b2',
                  fontWeight: 600,
                  fontSize: 13,
                  color: '#e65100'
                }}>
                  HARMONIZE
                </div>
                <div style={{ padding: 16 }}>
                  <div style={{ fontSize: 12, color: '#666', marginBottom: 8 }}>Output Type</div>
                  <div style={{ 
                    display: 'inline-block',
                    padding: '4px 8px', 
                    backgroundColor: '#e3f2fd', 
                    borderRadius: 4, 
                    fontSize: 12,
                    fontWeight: 500
                  }}>
                    {workflow.desiredOutputType}
                  </div>
                  
                  <div style={{ fontSize: 12, color: '#666', marginTop: 16, marginBottom: 4 }}>Context Prompt</div>
                  <div style={{ 
                    fontSize: 11, 
                    color: '#333',
                    backgroundColor: '#fafafa',
                    padding: 8,
                    borderRadius: 4,
                    maxHeight: 60,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis'
                  }}>
                    {workflow.contextPrompt.slice(0, 150)}...
                  </div>
                </div>
              </div>

              {/* Arrow */}
              <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <div style={{ fontSize: 20, color: '#999' }}>-&gt;</div>
              </div>

              {/* Outbound */}
              <div style={{ 
                backgroundColor: '#fff', 
                borderRadius: 8, 
                border: '1px solid #e0e0e0',
                overflow: 'hidden'
              }}>
                <div style={{ 
                  padding: '12px 16px', 
                  backgroundColor: '#e8f5e9', 
                  borderBottom: '1px solid #c8e6c9',
                  fontWeight: 600,
                  fontSize: 13,
                  color: '#2e7d32'
                }}>
                  OUTBOUND
                </div>
                <div style={{ padding: 16 }}>
                  <div style={{ fontSize: 12, color: '#666', marginBottom: 8 }}>Destination</div>
                  <div style={{ fontWeight: 500, marginBottom: 16 }}>Database</div>
                  
                  {teamPath ? (
                    <>
                      <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Organization Path</div>
                      <div style={{ 
                        display: 'flex', 
                        gap: 4, 
                        alignItems: 'center',
                        fontSize: 12,
                        marginBottom: 12,
                        flexWrap: 'wrap'
                      }}>
                        <span style={{ padding: '2px 6px', backgroundColor: '#e8f5e9', borderRadius: 4 }}>{teamPath.tenant}</span>
                        <span style={{ color: '#999' }}>/</span>
                        <span style={{ padding: '2px 6px', backgroundColor: '#e3f2fd', borderRadius: 4 }}>{teamPath.namespace}</span>
                        <span style={{ color: '#999' }}>/</span>
                        <span style={{ padding: '2px 6px', backgroundColor: '#fff3e0', borderRadius: 4 }}>{teamPath.team}</span>
                      </div>
                      
                      {generatedTableName && (
                        <>
                          <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Table Name</div>
                          <code style={{ 
                            display: 'block',
                            fontSize: 11, 
                            backgroundColor: '#263238', 
                            color: '#80cbc4',
                            padding: 8, 
                            borderRadius: 4,
                            marginBottom: 8
                          }}>
                            {generatedTableName}
                          </code>
                          <button
                            onClick={() => setShowTableModal(true)}
                            style={{
                              padding: '4px 10px',
                              backgroundColor: '#007bff',
                              color: 'white',
                              border: 'none',
                              borderRadius: 4,
                              cursor: 'pointer',
                              fontSize: 11
                            }}
                          >
                            View Table Info
                          </button>
                        </>
                      )}
                    </>
                  ) : (
                    <div style={{ fontSize: 12, color: '#999', fontStyle: 'italic' }}>
                      No team assigned. Assign a team to enable dynamic table creation.
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Recent Requests & Harmonized Output */}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
              {/* Incoming Requests */}
              <div style={{ 
                backgroundColor: '#fff', 
                borderRadius: 8, 
                border: '1px solid #e0e0e0',
                overflow: 'hidden'
              }}>
                <div style={{ 
                  padding: '12px 16px', 
                  backgroundColor: '#fafafa', 
                  borderBottom: '1px solid #e0e0e0',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center'
                }}>
                  <span style={{ fontWeight: 600, fontSize: 13 }}>Incoming Requests</span>
                  <span style={{ fontSize: 12, color: '#666' }}>{requests.length} total</span>
                </div>
                <div style={{ maxHeight: 300, overflow: 'auto' }}>
                  {loadingRequests ? (
                    <div style={{ padding: 16, color: '#666', textAlign: 'center' }}>Loading...</div>
                  ) : requests.length === 0 ? (
                    <div style={{ padding: 16, color: '#999', textAlign: 'center', fontSize: 13 }}>
                      No requests yet. Send data to the webhook URL above.
                    </div>
                  ) : (
                    requests.map(req => (
                      <div
                        key={req.id}
                        onClick={() => setSelectedRequest(req)}
                        style={{
                          padding: '10px 16px',
                          borderBottom: '1px solid #f0f0f0',
                          cursor: 'pointer',
                          backgroundColor: selectedRequest?.id === req.id ? '#e3f2fd' : 'transparent'
                        }}
                      >
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <span style={{ fontWeight: 500, fontSize: 13 }}>{req.source || 'Unknown'}</span>
                          <span style={{ fontSize: 11, color: '#999' }}>#{req.id}</span>
                        </div>
                        <div style={{ fontSize: 11, color: '#666', marginTop: 2 }}>
                          {new Date(req.createdAt).toLocaleString()}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>

              {/* Harmonized Output */}
              <div style={{ 
                backgroundColor: '#fff', 
                borderRadius: 8, 
                border: '1px solid #e0e0e0',
                overflow: 'hidden'
              }}>
                <div style={{ 
                  padding: '12px 16px', 
                  backgroundColor: '#fafafa', 
                  borderBottom: '1px solid #e0e0e0',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center'
                }}>
                  <span style={{ fontWeight: 600, fontSize: 13 }}>Harmonized Output</span>
                  {selectedRequest && (
                    <span style={{ fontSize: 12, color: '#666' }}>Request #{selectedRequest.id}</span>
                  )}
                </div>
                <div style={{ maxHeight: 300, overflow: 'auto' }}>
                  {!selectedRequest ? (
                    <div style={{ padding: 16, color: '#999', textAlign: 'center', fontSize: 13 }}>
                      Select a request to view harmonized output
                    </div>
                  ) : records.length === 0 ? (
                    <div style={{ padding: 16 }}>
                      <div style={{ color: '#999', marginBottom: 12, fontSize: 13 }}>
                        Not yet harmonized
                      </div>
                      <div style={{ fontSize: 12, color: '#666', marginBottom: 8 }}>Raw Payload:</div>
                      <pre style={{ 
                        backgroundColor: '#f5f5f5', 
                        padding: 12, 
                        borderRadius: 4, 
                        fontSize: 11,
                        margin: 0,
                        maxHeight: 200,
                        overflow: 'auto'
                      }}>
                        {formatJson(selectedRequest.payload)}
                      </pre>
                    </div>
                  ) : (
                    <div style={{ padding: 16 }}>
                      {records.map(record => (
                        <div key={record.id}>
                          <div style={{ fontSize: 12, color: '#666', marginBottom: 8 }}>
                            Harmonized at {new Date(record.receivedAt).toLocaleString()}
                          </div>
                          <pre style={{ 
                            backgroundColor: '#263238', 
                            color: '#a5d6a7',
                            padding: 12, 
                            borderRadius: 4, 
                            fontSize: 11,
                            margin: 0,
                            maxHeight: 200,
                            overflow: 'auto'
                          }}>
                            {record.harmonizedJson ? formatJson(record.harmonizedJson) : 'No output'}
                          </pre>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'overview' && (
          <div style={{ padding: 24 }}>
            {/* Webhook URL */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
              <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>Webhook URL</h4>
              <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                <code style={{ flex: 1, backgroundColor: '#f5f5f5', padding: 10, borderRadius: 4, fontSize: 12, wordBreak: 'break-all' }}>
                  {webhookUrl}
                </code>
                <button onClick={() => copyToClipboard(webhookUrl)} style={{ padding: '8px 14px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12 }}>
                  Copy
                </button>
              </div>
            </div>

            {/* API Key */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
              <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>API Key</h4>
              <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                <code style={{ flex: 1, backgroundColor: '#f5f5f5', padding: 10, borderRadius: 4, fontSize: 12 }}>
                  {workflow.apiKey}
                </code>
                <button onClick={() => copyToClipboard(workflow.apiKey)} style={{ padding: '8px 14px', backgroundColor: '#6c757d', color: 'white', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12 }}>
                  Copy
                </button>
                <button onClick={handleRegenerateKey} disabled={regenerating} style={{ padding: '8px 14px', backgroundColor: regenerating ? '#ccc' : '#ff9800', color: 'white', border: 'none', borderRadius: 4, cursor: regenerating ? 'default' : 'pointer', fontSize: 12 }}>
                  {regenerating ? 'Regenerating...' : 'Regenerate'}
                </button>
              </div>
            </div>

            {/* Metadata */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
              <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>Metadata</h4>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, fontSize: 13 }}>
                <div>
                  <span style={{ color: '#666' }}>Created:</span>{' '}
                  {new Date(workflow.createdAt).toLocaleString()}
                </div>
                {workflow.updatedAt && (
                  <div>
                    <span style={{ color: '#666' }}>Updated:</span>{' '}
                    {new Date(workflow.updatedAt).toLocaleString()}
                  </div>
                )}
                <div>
                  <span style={{ color: '#666' }}>Requests:</span> {requests.length}
                </div>
                <div>
                  <span style={{ color: '#666' }}>Output Type:</span> {workflow.desiredOutputType}
                </div>
              </div>
            </div>

            {/* API Usage Examples */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16 }}>
              <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>API Usage Examples</h4>
              
              {/* cURL Example */}
              <div style={{ marginBottom: 16 }}>
                <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 6, color: '#555' }}>cURL</div>
                <pre style={{
                  backgroundColor: '#263238',
                  color: '#a5d6a7',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 11,
                  margin: 0,
                  overflow: 'auto'
                }}>
{`curl -X POST "${webhookUrl}" \\
  -H "Content-Type: application/json" \\
  -d '{
    "your": "data",
    "goes": "here",
    "nested": {
      "values": "supported"
    }
  }'`}
                </pre>
                <button
                  onClick={() => copyToClipboard(`curl -X POST "${webhookUrl}" -H "Content-Type: application/json" -d '{"your": "data", "goes": "here"}'`)}
                  style={{
                    marginTop: 8,
                    padding: '6px 12px',
                    backgroundColor: '#6c757d',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 11
                  }}
                >
                  Copy cURL Command
                </button>
              </div>

              {/* JavaScript/Fetch Example */}
              <div style={{ marginBottom: 16 }}>
                <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 6, color: '#555' }}>JavaScript (Fetch API)</div>
                <pre style={{
                  backgroundColor: '#263238',
                  color: '#a5d6a7',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 11,
                  margin: 0,
                  overflow: 'auto'
                }}>
{`fetch("${webhookUrl}", {
  method: "POST",
  headers: {
    "Content-Type": "application/json"
  },
  body: JSON.stringify({
    your: "data",
    goes: "here"
  })
})
  .then(res => res.json())
  .then(data => console.log(data));`}
                </pre>
              </div>

              {/* Python Example */}
              <div style={{ marginBottom: 16 }}>
                <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 6, color: '#555' }}>Python (requests)</div>
                <pre style={{
                  backgroundColor: '#263238',
                  color: '#a5d6a7',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 11,
                  margin: 0,
                  overflow: 'auto'
                }}>
{`import requests

data = {
    "your": "data",
    "goes": "here"
}

response = requests.post(
    "${webhookUrl}",
    json=data
)

print(response.json())`}
                </pre>
              </div>

              {/* Response Structure */}
              <div>
                <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 6, color: '#555' }}>Expected Response</div>
                <pre style={{
                  backgroundColor: '#f5f5f5',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 11,
                  margin: 0,
                  overflow: 'auto',
                  color: '#333'
                }}>
{`{
  "success": true,
  "requestId": 123,
  "recordId": 456,
  "harmonizedData": {
    // Your harmonized output
    // based on context prompt
  }
}`}
                </pre>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'database' && (
          <div style={{ padding: 24 }}>
            {generatedTableName && teamPath ? (
              <>
                {/* Table Information */}
                <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
                  <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>Database Table</h4>
                  
                  <div style={{ marginBottom: 12 }}>
                    <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Organization Path</div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13 }}>
                      <span style={{ padding: '4px 10px', backgroundColor: '#e8f5e9', borderRadius: 4, fontWeight: 500 }}>
                        {teamPath.tenant}
                      </span>
                      <span style={{ color: '#999' }}>/</span>
                      <span style={{ padding: '4px 10px', backgroundColor: '#e3f2fd', borderRadius: 4, fontWeight: 500 }}>
                        {teamPath.namespace}
                      </span>
                      <span style={{ color: '#999' }}>/</span>
                      <span style={{ padding: '4px 10px', backgroundColor: '#fff3e0', borderRadius: 4, fontWeight: 500 }}>
                        {teamPath.team}
                      </span>
                    </div>
                  </div>
                  
                  <div style={{ marginBottom: 12 }}>
                    <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Table Name</div>
                    <code style={{ 
                      display: 'inline-block',
                      fontSize: 13, 
                      backgroundColor: '#263238', 
                      color: '#80cbc4',
                      padding: '8px 12px', 
                      borderRadius: 4,
                      fontWeight: 500
                    }}>
                      {generatedTableName}
                    </code>
                  </div>
                  
                  <div>
                    <div style={{ fontSize: 12, color: '#666', marginBottom: 4 }}>Entity Name</div>
                    <div style={{ fontSize: 13, fontWeight: 500 }}>{workflow.entityName}</div>
                  </div>
                </div>

                {/* Harmonized Records */}
                <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', overflow: 'hidden' }}>
                  <div style={{ 
                    padding: '12px 16px', 
                    backgroundColor: '#fafafa', 
                    borderBottom: '1px solid #e0e0e0',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                  }}>
                    <span style={{ fontWeight: 600, fontSize: 14 }}>Harmonized Records</span>
                    <span style={{ fontSize: 12, color: '#666' }}>
                      {allWorkflowRecords.length} record{allWorkflowRecords.length !== 1 ? 's' : ''}
                    </span>
                  </div>
                  
                  <div style={{ maxHeight: 500, overflow: 'auto' }}>
                    {allWorkflowRecords.length === 0 ? (
                      <div style={{ padding: 24, textAlign: 'center', color: '#999' }}>
                        <div style={{ marginBottom: 8 }}>No harmonized records yet</div>
                        <div style={{ fontSize: 12 }}>
                          Send data to the webhook URL or use manual data entry to create records
                        </div>
                      </div>
                    ) : (
                      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                        <thead>
                          <tr style={{ backgroundColor: '#f5f5f5' }}>
                            <th style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '1px solid #e0e0e0', fontWeight: 600 }}>ID</th>
                            <th style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '1px solid #e0e0e0', fontWeight: 600 }}>Created At</th>
                            <th style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '1px solid #e0e0e0', fontWeight: 600 }}>Request ID</th>
                            <th style={{ padding: '10px 12px', textAlign: 'left', borderBottom: '1px solid #e0e0e0', fontWeight: 600 }}>Data</th>
                          </tr>
                        </thead>
                        <tbody>
                          {allWorkflowRecords.map((record, index) => (
                            <tr key={record.id} style={{ 
                              backgroundColor: index % 2 === 0 ? '#fff' : '#fafafa',
                              borderBottom: '1px solid #f0f0f0'
                            }}>
                              <td style={{ padding: '10px 12px' }}>{record.id}</td>
                              <td style={{ padding: '10px 12px', fontSize: 12 }}>
                                {new Date(record.receivedAt).toLocaleString()}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                {record.harmonizeRequestId || '-'}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                <details>
                                  <summary style={{ cursor: 'pointer', color: '#007bff' }}>
                                    View JSON
                                  </summary>
                                  <pre style={{
                                    marginTop: 8,
                                    padding: 8,
                                    backgroundColor: '#263238',
                                    color: '#a5d6a7',
                                    borderRadius: 4,
                                    fontSize: 11,
                                    overflow: 'auto',
                                    maxHeight: 200
                                  }}>
                                    {record.harmonizedJson ? formatJson(record.harmonizedJson) : 'No data'}
                                  </pre>
                                </details>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    )}
                  </div>
                </div>
              </>
            ) : (
              <div style={{ 
                backgroundColor: '#fff3e0', 
                border: '1px solid #ffe0b2',
                borderRadius: 8,
                padding: 24,
                textAlign: 'center'
              }}>
                <div style={{ fontSize: 14, fontWeight: 600, color: '#e65100', marginBottom: 8 }}>
                  No Database Table Configured
                </div>
                <div style={{ fontSize: 13, color: '#666', marginBottom: 16 }}>
                  This workflow is not assigned to a team or does not have an entity name configured.
                </div>
                <button
                  onClick={onEdit}
                  style={{
                    padding: '8px 16px',
                    backgroundColor: '#ff9800',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 13
                  }}
                >
                  Configure Team & Entity
                </button>
              </div>
            )}
          </div>
        )}

        {activeTab === 'config' && (
          <div style={{ padding: 24 }}>
            {/* Prompt Templates Management */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <h4 style={{ margin: 0, fontSize: 14 }}>Prompt Templates</h4>
                <button
                  onClick={() => {/* TODO: Open create template modal */}}
                  style={{
                    padding: '6px 12px',
                    backgroundColor: '#28a745',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 12
                  }}
                >
                  + Create Template
                </button>
              </div>
              
              <div style={{ 
                padding: 12, 
                backgroundColor: '#e3f2fd', 
                borderRadius: 4,
                marginBottom: 12,
                fontSize: 12,
                color: '#1565c0'
              }}>
                Create and manage prompt templates specific to this workflow. Templates define how incoming data is harmonized.
              </div>

              {/* Template list placeholder */}
              <div style={{ 
                padding: 16, 
                backgroundColor: '#f5f5f5', 
                borderRadius: 4,
                textAlign: 'center',
                color: '#666',
                fontSize: 13
              }}>
                No templates yet. Click "Create Template" to add your first prompt template.
              </div>
            </div>

            {/* Current Active Prompt */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16, marginBottom: 16 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                <h4 style={{ margin: 0, fontSize: 14 }}>Active Context Prompt</h4>
                <button
                  onClick={onEdit}
                  style={{
                    padding: '4px 10px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 11
                  }}
                >
                  Edit
                </button>
              </div>
              <pre style={{ 
                backgroundColor: '#f5f5f5', 
                padding: 12, 
                borderRadius: 4, 
                margin: 0, 
                fontSize: 11, 
                whiteSpace: 'pre-wrap', 
                maxHeight: 150, 
                overflow: 'auto',
                border: '1px solid #e0e0e0'
              }}>
                {workflow.contextPrompt || 'No context prompt configured'}
              </pre>
            </div>

            {/* Current Active Output Schema */}
            <div style={{ backgroundColor: '#fff', borderRadius: 8, border: '1px solid #e0e0e0', padding: 16 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                <h4 style={{ margin: 0, fontSize: 14 }}>
                  Active Desired Output Schema
                  <span style={{ 
                    marginLeft: 8, 
                    fontSize: 11, 
                    padding: '2px 8px',
                    backgroundColor: '#e3f2fd', 
                    borderRadius: 4,
                    fontWeight: 400
                  }}>
                    {workflow.desiredOutputType}
                  </span>
                </h4>
                <button
                  onClick={onEdit}
                  style={{
                    padding: '4px 10px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 11
                  }}
                >
                  Edit
                </button>
              </div>
              <pre style={{ 
                backgroundColor: '#263238', 
                color: '#80cbc4',
                padding: 12, 
                borderRadius: 4, 
                margin: 0, 
                fontSize: 11, 
                whiteSpace: 'pre-wrap', 
                maxHeight: 250, 
                overflow: 'auto',
                fontFamily: 'monospace',
                border: '1px solid #37474f'
              }}>
                {workflow.desiredOutput || '{}'}
              </pre>
            </div>
          </div>
        )}
      </div>

      {/* Table Information Modal */}
      {showTableModal && generatedTableName && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0,0,0,0.5)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000
        }}>
          <div style={{
            backgroundColor: 'white',
            borderRadius: 8,
            width: '90%',
            maxWidth: 800,
            maxHeight: '80vh',
            overflow: 'auto'
          }}>
            <div style={{
              padding: '16px 20px',
              borderBottom: '1px solid #e0e0e0',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              position: 'sticky',
              top: 0,
              backgroundColor: 'white',
              zIndex: 1
            }}>
              <h3 style={{ margin: 0, fontSize: 16 }}>Database Table Information</h3>
              <button
                onClick={() => setShowTableModal(false)}
                style={{
                  background: 'none',
                  border: 'none',
                  fontSize: 24,
                  cursor: 'pointer',
                  color: '#666'
                }}
              >
                ×
              </button>
            </div>

            <div style={{ padding: 20 }}>
              {/* Table Name */}
              <div style={{ marginBottom: 20 }}>
                <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold', fontSize: 13 }}>
                  Table Name
                </label>
                <div style={{
                  backgroundColor: '#263238',
                  color: '#80cbc4',
                  padding: 12,
                  borderRadius: 4,
                  fontFamily: 'monospace',
                  fontSize: 14
                }}>
                  {generatedTableName}
                </div>
              </div>

              {/* Organization Path */}
              {teamPath && (
                <div style={{ marginBottom: 20 }}>
                  <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold', fontSize: 13 }}>
                    Organization Hierarchy
                  </label>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13 }}>
                    <span style={{ padding: '4px 10px', backgroundColor: '#e8f5e9', borderRadius: 4, fontWeight: 500 }}>
                      {teamPath.tenant}
                    </span>
                    <span style={{ color: '#999' }}>/</span>
                    <span style={{ padding: '4px 10px', backgroundColor: '#e3f2fd', borderRadius: 4, fontWeight: 500 }}>
                      {teamPath.namespace}
                    </span>
                    <span style={{ color: '#999' }}>/</span>
                    <span style={{ padding: '4px 10px', backgroundColor: '#fff3e0', borderRadius: 4, fontWeight: 500 }}>
                      {teamPath.team}
                    </span>
                  </div>
                </div>
              )}

              {/* Schema Preview */}
              <div style={{ marginBottom: 20 }}>
                <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold', fontSize: 13 }}>
                  Expected Schema (from Desired Output)
                </label>
                <div style={{
                  backgroundColor: '#f5f5f5',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 12,
                  border: '1px solid #e0e0e0'
                }}>
                  <div style={{ marginBottom: 8, color: '#666' }}>Standard Columns:</div>
                  <div style={{ fontFamily: 'monospace', marginLeft: 16 }}>
                    <div>• id (INTEGER, PRIMARY KEY, AUTOINCREMENT)</div>
                    <div>• created_at (DATETIME)</div>
                    <div>• workflow_id (INTEGER)</div>
                    <div>• request_id (INTEGER)</div>
                  </div>
                  <div style={{ marginTop: 12, marginBottom: 8, color: '#666' }}>Data Columns (from schema):</div>
                  <div style={{ fontFamily: 'monospace', marginLeft: 16, color: '#555' }}>
                    Based on your desired output schema, columns will be auto-generated from the JSON structure
                  </div>
                </div>
              </div>

              {/* Table Status */}
              <div style={{
                backgroundColor: '#e8f5e9',
                border: '1px solid #c8e6c9',
                borderRadius: 4,
                padding: 12
              }}>
                <div style={{ fontWeight: 'bold', marginBottom: 4, fontSize: 13, color: '#2e7d32' }}>
                  Table Creation
                </div>
                <div style={{ fontSize: 12, color: '#1b5e20' }}>
                  The table will be automatically created when the first harmonization request is processed for this workflow.
                  The schema is dynamically generated from the desired output structure.
                </div>
              </div>

              {/* SQL Preview */}
              <div style={{ marginTop: 20 }}>
                <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold', fontSize: 13 }}>
                  Sample CREATE TABLE Statement
                </label>
                <pre style={{
                  backgroundColor: '#263238',
                  color: '#a5d6a7',
                  padding: 12,
                  borderRadius: 4,
                  fontSize: 11,
                  overflow: 'auto',
                  margin: 0,
                  fontFamily: 'monospace'
                }}>
{`CREATE TABLE IF NOT EXISTS "${generatedTableName}" (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  workflow_id INTEGER,
  request_id INTEGER,
  -- Columns auto-generated from JSON schema:
  -- device_id TEXT,
  -- device_type TEXT,
  -- timestamp TEXT,
  -- status_operational INTEGER,
  -- status_health REAL,
  -- ... (based on your desired output structure)
);`}
                </pre>
              </div>

              {/* Action Buttons */}
              <div style={{ marginTop: 20, display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                <button
                  onClick={() => setShowTableModal(false)}
                  style={{
                    padding: '8px 16px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 13
                  }}
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WorkflowDetails;
