import React, { useState, useEffect } from 'react';
import { fetchWorkflows, getWorkflowWebhookUrl } from '../../api/harmonizer';
import type { Workflow } from '../../types';

interface HarmonizeResult {
  requestId?: number;
  harmonizedData?: Record<string, unknown>;
}

const ManualDataEntry: React.FC = () => {
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  const [selectedWorkflowId, setSelectedWorkflowId] = useState<string>('');
  const [payload, setPayload] = useState<string>('');
  const [source, setSource] = useState<string>('Manual Entry');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<{ success: boolean; data?: HarmonizeResult; error?: string } | null>(null);

  useEffect(() => {
    fetchWorkflows()
      .then(data => {
        setWorkflows(data.filter(w => w.isActive));
      })
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async () => {
    if (!selectedWorkflowId || !payload.trim()) return;
    
    const workflow = workflows.find(w => w.id === parseInt(selectedWorkflowId));
    if (!workflow) return;

    setSubmitting(true);
    setResult(null);

    try {
      // Send to webhook endpoint
      const webhookUrl = getWorkflowWebhookUrl(workflow.apiKey);
      const response = await fetch(webhookUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: payload
      });
      
      const data = await response.json();
      
      if (response.ok) {
        setResult({ success: true, data });
        setPayload('');
      } else {
        setResult({ success: false, error: data.error || 'Failed to process data' });
      }
    } catch (err) {
      setResult({ success: false, error: (err as Error).message });
    } finally {
      setSubmitting(false);
    }
  };

  const formatJson = () => {
    try {
      const parsed = JSON.parse(payload);
      setPayload(JSON.stringify(parsed, null, 2));
    } catch {
      // Not valid JSON, ignore
    }
  };

  const loadSampleData = () => {
    const sample = {
      machineId: "CNC-001",
      timestamp: new Date().toISOString(),
      temperature: 45.2,
      pressure: 101.3,
      status: "running",
      metrics: {
        partsProduced: 150,
        efficiency: 0.92
      }
    };
    setPayload(JSON.stringify(sample, null, 2));
  };

  if (loading) return <div style={{ padding: 24 }}>Loading...</div>;

  return (
    <div style={{ padding: 24, maxWidth: 900, margin: '0 auto' }}>
      <h2>Manual Data Entry</h2>
      <p style={{ color: '#666' }}>
        Submit data manually to a workflow for harmonization. This simulates data coming from an external source.
      </p>

      {/* Workflow Selection */}
      <div style={{ marginBottom: 20 }}>
        <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold' }}>
          Select Workflow
        </label>
        <select
          value={selectedWorkflowId}
          onChange={e => setSelectedWorkflowId(e.target.value)}
          style={{ width: '100%', padding: 10, fontSize: 14 }}
        >
          <option value="">-- Choose a workflow --</option>
          {workflows.map(w => (
            <option key={w.id} value={w.id}>
              {w.name} ({w.desiredOutputType})
            </option>
          ))}
        </select>
        {selectedWorkflowId && (
          <p style={{ fontSize: 12, color: '#666', marginTop: 4 }}>
            {workflows.find(w => w.id === parseInt(selectedWorkflowId))?.description}
          </p>
        )}
      </div>

      {/* Source Label */}
      <div style={{ marginBottom: 20 }}>
        <label style={{ display: 'block', marginBottom: 8, fontWeight: 'bold' }}>
          Source Label
        </label>
        <input
          type="text"
          value={source}
          onChange={e => setSource(e.target.value)}
          style={{ width: '100%', padding: 10, boxSizing: 'border-box' }}
          placeholder="e.g., Manual Entry, Test Data, Customer Upload"
        />
      </div>

      {/* Data Input */}
      <div style={{ marginBottom: 20 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 }}>
          <label style={{ fontWeight: 'bold' }}>Data Payload (JSON)</label>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              onClick={loadSampleData}
              style={{
                padding: '4px 12px',
                backgroundColor: '#6c757d',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: 'pointer',
                fontSize: 12
              }}
            >
              Load Sample
            </button>
            <button
              onClick={formatJson}
              style={{
                padding: '4px 12px',
                backgroundColor: '#17a2b8',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: 'pointer',
                fontSize: 12
              }}
            >
              Format JSON
            </button>
          </div>
        </div>
        <textarea
          value={payload}
          onChange={e => setPayload(e.target.value)}
          rows={12}
          style={{ 
            width: '100%', 
            padding: 12, 
            fontFamily: 'monospace', 
            fontSize: 13,
            boxSizing: 'border-box',
            border: '1px solid #ddd',
            borderRadius: 4
          }}
          placeholder='{"your": "data", "goes": "here"}'
        />
      </div>

      {/* Submit Button */}
      <button
        onClick={handleSubmit}
        disabled={!selectedWorkflowId || !payload.trim() || submitting}
        style={{
          padding: '12px 24px',
          backgroundColor: (!selectedWorkflowId || !payload.trim() || submitting) ? '#ccc' : '#28a745',
          color: 'white',
          border: 'none',
          borderRadius: 4,
          cursor: (!selectedWorkflowId || !payload.trim() || submitting) ? 'default' : 'pointer',
          fontSize: 16,
          fontWeight: 'bold'
        }}
      >
        {submitting ? 'Processing...' : 'Submit & Harmonize'}
      </button>

      {/* Result Display */}
      {result && (
        <div style={{ 
          marginTop: 24,
          padding: 16,
          borderRadius: 8,
          backgroundColor: result.success ? '#d4edda' : '#f8d7da',
          border: `1px solid ${result.success ? '#c3e6cb' : '#f5c6cb'}`
        }}>
          <h4 style={{ 
            margin: '0 0 12px', 
            color: result.success ? '#155724' : '#721c24' 
          }}>
            {result.success ? 'Harmonization Successful' : 'Harmonization Failed'}
          </h4>
          
          {result.success && result.data && (
            <div>
              <p style={{ fontSize: 14, marginBottom: 8 }}>
                <strong>Request ID:</strong> {(result.data as { requestId?: number }).requestId}
              </p>
              <p style={{ fontSize: 14, fontWeight: 'bold', marginBottom: 4 }}>Harmonized Output:</p>
              <pre style={{
                backgroundColor: 'white',
                padding: 12,
                borderRadius: 4,
                overflow: 'auto',
                maxHeight: 300,
                fontSize: 12
              }}>
                {JSON.stringify((result.data as Record<string, unknown>).harmonizedData, null, 2) ?? 'null'}
              </pre>
            </div>
          )}
          
          {result.error && (
            <p style={{ margin: 0, color: '#721c24' }}>{result.error}</p>
          )}
        </div>
      )}
    </div>
  );
};

export default ManualDataEntry;
