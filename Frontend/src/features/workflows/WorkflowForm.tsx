import React, { useState, useEffect } from 'react';
import { createWorkflow, updateWorkflow, fetchPrompts, fetchTenantHierarchy, getWorkflowTableName } from '../../api/harmonizer';
import type { Workflow, Prompt, WorkflowCreateDto, WorkflowUpdateDto, TenantHierarchy } from '../../types';

interface WorkflowFormProps {
  workflow: Workflow | null;
  onClose: (saved: boolean) => void;
}

interface FormData {
  name: string;
  description: string;
  contextPrompt: string;
  desiredOutput: string;
  desiredOutputType: string;
  isActive: boolean;
  teamId: number | null;
  entityName: string;
  outboundApiEnabled: boolean;
  outboundApiUrl: string;
  outboundApiHeaders: string;
}

const WorkflowForm: React.FC<WorkflowFormProps> = ({ workflow, onClose }) => {
  const [formData, setFormData] = useState<FormData>({
    name: '',
    description: '',
    contextPrompt: '',
    desiredOutput: '',
    desiredOutputType: 'JSON',
    isActive: true,
    teamId: null,
    entityName: '',
    outboundApiEnabled: false,
    outboundApiUrl: '',
    outboundApiHeaders: ''
  });
  const [prompts, setPrompts] = useState<Prompt[]>([]);
  const [hierarchy, setHierarchy] = useState<TenantHierarchy[]>([]);
  const [selectedPromptId, setSelectedPromptId] = useState<string>('');
  const [saving, setSaving] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [existingTableName, setExistingTableName] = useState<string | null>(null);

  useEffect(() => {
    if (workflow) {
      setFormData({
        name: workflow.name || '',
        description: workflow.description || '',
        contextPrompt: workflow.contextPrompt || '',
        desiredOutput: workflow.desiredOutput || '',
        desiredOutputType: workflow.desiredOutputType || 'JSON',
        isActive: workflow.isActive ?? true,
        teamId: workflow.teamId || null,
        entityName: workflow.entityName || '',
        outboundApiEnabled: workflow.outboundApiEnabled || false,
        outboundApiUrl: workflow.outboundApiUrl || '',
        outboundApiHeaders: workflow.outboundApiHeaders || ''
      });
      
      // Fetch existing table name if workflow has team and entity
      if (workflow.teamId && workflow.entityName) {
        getWorkflowTableName(workflow.id).then(result => {
          if (result.exists && result.tableName) {
            setExistingTableName(result.tableName);
          }
        }).catch(console.error);
      }
    }
  }, [workflow]);

  useEffect(() => {
    fetchPrompts().then(setPrompts).catch(console.error);
    fetchTenantHierarchy().then(setHierarchy).catch(console.error);
  }, []);

  const handlePromptSelect = (e: React.ChangeEvent<HTMLSelectElement>): void => {
    const promptId = e.target.value;
    setSelectedPromptId(promptId);
    
    if (promptId) {
      const selected = prompts.find(p => p.id === parseInt(promptId));
      if (selected) {
        setFormData(prev => ({
          ...prev,
          contextPrompt: selected.contextPrompt,
          desiredOutput: selected.desiredOutput,
          desiredOutputType: selected.desiredOutputType?.toUpperCase() || 'JSON'
        }));
      }
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ): void => {
    const target = e.target;
    const name = target.name;
    const value = target.type === 'checkbox' ? (target as HTMLInputElement).checked : target.value;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault();
    setError('');
    setSaving(true);

    try {
      if (workflow) {
        const updateData: WorkflowUpdateDto = {
          name: formData.name,
          description: formData.description,
          contextPrompt: formData.contextPrompt || 'Transform the incoming data.',
          desiredOutput: formData.desiredOutput || '{}',
          desiredOutputType: formData.desiredOutputType,
          isActive: formData.isActive,
          teamId: formData.teamId,
          entityName: formData.entityName || null,
          outboundApiEnabled: formData.outboundApiEnabled,
          outboundApiUrl: formData.outboundApiUrl || null,
          outboundApiHeaders: formData.outboundApiHeaders || null
        };
        await updateWorkflow(workflow.id, updateData);
      } else {
        const createData: WorkflowCreateDto = {
          name: formData.name,
          description: formData.description,
          contextPrompt: formData.contextPrompt || 'Transform the incoming data.',
          desiredOutput: formData.desiredOutput || '{}',
          desiredOutputType: formData.desiredOutputType,
          teamId: formData.teamId,
          entityName: formData.entityName || null
        };
        await createWorkflow(createData);
      }
      onClose(true);
    } catch (err) {
      setError((err as Error).message || 'Failed to save workflow');
    } finally {
      setSaving(false);
    }
  };

  return (
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
        maxWidth: 600,
        maxHeight: '90vh',
        overflow: 'auto'
      }}>
        <div style={{
          padding: 20,
          borderBottom: '1px solid #ddd',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <h2 style={{ margin: 0 }}>
            {workflow ? 'Edit Workflow' : 'Create New Workflow'}
          </h2>
          <button
            onClick={() => onClose(false)}
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

        <form onSubmit={handleSubmit} style={{ padding: 20 }}>
          {error && (
            <div style={{
              padding: 12,
              backgroundColor: '#ffebee',
              color: '#c62828',
              borderRadius: 4,
              marginBottom: 16
            }}>
              {error}
            </div>
          )}

          <div style={{ marginBottom: 16 }}>
            <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
              Workflow Name *
            </label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
              style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
              placeholder="e.g., IoT Sensor Data Processor"
            />
          </div>

          <div style={{ marginBottom: 16 }}>
            <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
              Description
            </label>
            <input
              type="text"
              name="description"
              value={formData.description}
              onChange={handleChange}
              style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
              placeholder="Brief description of what this workflow does"
            />
          </div>

          {/* Info about prompt configuration */}
          {!workflow && (
            <div style={{
              padding: 12,
              backgroundColor: '#e3f2fd',
              border: '1px solid #90caf9',
              borderRadius: 4,
              marginBottom: 16,
              fontSize: 13
            }}>
              <div style={{ fontWeight: 600, marginBottom: 4, color: '#1565c0' }}>
                Prompt Configuration
              </div>
              <div style={{ color: '#424242' }}>
                After creating the workflow, configure context prompts and output schema in the <strong>Config</strong> tab. 
                You can create and manage prompt templates specific to this workflow.
              </div>
            </div>
          )}

          {/* Prompt editing section (only in edit mode) */}
          {workflow && (
            <div style={{ 
              backgroundColor: '#fff', 
              border: '1px solid #e0e0e0',
              borderRadius: 8,
              padding: 16,
              marginBottom: 16
            }}>
              <h4 style={{ margin: '0 0 12px', fontSize: 14 }}>
                Prompt Configuration
              </h4>

              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                  Context Prompt
                </label>
                <textarea
                  name="contextPrompt"
                  value={formData.contextPrompt}
                  onChange={handleChange}
                  rows={4}
                  style={{ width: '100%', padding: 8, boxSizing: 'border-box', fontSize: 12 }}
                  placeholder="Enter the instructions for harmonizing incoming data..."
                />
              </div>

              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                  Desired Output Type
                </label>
                <select
                  name="desiredOutputType"
                  value={formData.desiredOutputType}
                  onChange={handleChange}
                  style={{ width: '100%', padding: 8 }}
                >
                  <option value="JSON">JSON</option>
                  <option value="TEXT">Text</option>
                  <option value="VARIABLE">Variable</option>
                </select>
              </div>

              <div style={{ marginBottom: 0 }}>
                <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                  Desired Output Schema
                </label>
                <textarea
                  name="desiredOutput"
                  value={formData.desiredOutput}
                  onChange={handleChange}
                  rows={6}
                  style={{ 
                    width: '100%', 
                    padding: 8, 
                    boxSizing: 'border-box',
                    fontFamily: formData.desiredOutputType === 'JSON' ? 'monospace' : 'inherit',
                    fontSize: 12
                  }}
                  placeholder={formData.desiredOutputType === 'JSON' 
                    ? '{\n  "field1": "description",\n  "field2": "description"\n}'
                    : 'Describe the expected output format...'
                  }
                />
              </div>
            </div>
          )}

          <div style={{ 
            padding: 16, 
            backgroundColor: '#f8f9fa',
            backgroundColor: '#f8f9fa', 
            borderRadius: 8, 
            marginBottom: 16,
            border: '1px solid #e0e0e0'
          }}>
            <h4 style={{ margin: '0 0 12px', fontSize: 14, color: '#333' }}>Outbound Configuration</h4>
            
            <div style={{ marginBottom: 16 }}>
              <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                Team (for dynamic table creation)
              </label>
              <select
                name="teamId"
                value={formData.teamId || ''}
                onChange={(e) => setFormData(prev => ({ 
                  ...prev, 
                  teamId: e.target.value ? parseInt(e.target.value) : null 
                }))}
                style={{ width: '100%', padding: 8 }}
              >
                <option value="">-- No team (no automatic table creation) --</option>
                {hierarchy.map(tenant => 
                  tenant.namespaces.map(ns => 
                    ns.teams.map(team => (
                      <option key={team.id} value={team.id}>
                        {tenant.name} / {ns.name} / {team.name}
                      </option>
                    ))
                  )
                )}
              </select>
              <p style={{ fontSize: 12, color: '#666', margin: '4px 0 0' }}>
                Assigning a team enables automatic database table creation for harmonized data
              </p>
            </div>

            <div style={{ marginBottom: 0 }}>
              <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                Entity Name (for table naming)
              </label>
              <input
                type="text"
                name="entityName"
                value={formData.entityName}
                onChange={handleChange}
                style={{ 
                  width: '100%', 
                  padding: 8, 
                  boxSizing: 'border-box',
                  borderColor: formData.teamId && !formData.entityName ? '#ff9800' : '#ddd'
                }}
                placeholder="e.g., sensor_readings, orders, events"
              />
              
              {/* Show existing table name */}
              {existingTableName && workflow && (
                <div style={{ 
                  marginTop: 8,
                  padding: 8,
                  backgroundColor: '#e8f5e9',
                  border: '1px solid #c8e6c9',
                  borderRadius: 4
                }}>
                  <div style={{ fontSize: 12, fontWeight: 'bold', color: '#2e7d32', marginBottom: 4 }}>
                    Existing Table
                  </div>
                  <code style={{ 
                    fontSize: 11,
                    backgroundColor: '#fff',
                    padding: '4px 8px',
                    borderRadius: 4,
                    color: '#1b5e20',
                    fontWeight: 500
                  }}>
                    {existingTableName}
                  </code>
                  <div style={{ fontSize: 11, color: '#388e3c', marginTop: 4 }}>
                    This table already exists with data. Changing team/entity will create a new table.
                  </div>
                </div>
              )}
              
              {formData.teamId && !formData.entityName && (
                <p style={{ fontSize: 12, color: '#ff9800', margin: '4px 0', fontWeight: 500 }}>
                  Entity name is recommended when a team is selected
                </p>
              )}
              <p style={{ fontSize: 12, color: '#666', margin: '4px 0 0' }}>
                {formData.teamId && formData.entityName ? (
                  <>
                    {existingTableName ? 'New table will be named: ' : 'Table will be named: '}<code style={{ 
                      backgroundColor: '#263238', 
                      color: '#80cbc4', 
                      padding: '2px 6px', 
                      borderRadius: 4,
                      fontWeight: 500
                    }}>
                      {(() => {
                        const team = hierarchy
                          .flatMap(t => t.namespaces.flatMap(n => n.teams))
                          .find(tm => tm.id === formData.teamId);
                        if (!team) return 'unknown';
                        const ns = hierarchy
                          .flatMap(t => t.namespaces)
                          .find(n => n.teams.some(tm => tm.id === formData.teamId));
                        const tenant = hierarchy
                          .find(t => t.namespaces.some(n => n.id === ns?.id));
                        return `${tenant?.name.toLowerCase().replace(/\s+/g, '_')}_${ns?.name.toLowerCase()}_${team.name.toLowerCase()}_${formData.entityName.toLowerCase().replace(/\s+/g, '_')}`;
                      })()}
                    </code>
                  </>
                ) : formData.teamId ? (
                  'Enter an entity name to see the generated table name'
                ) : (
                  'Select a team above to enable automatic table creation'
                )}
              </p>
            </div>
          </div>

          {/* Outbound API Configuration */}
          <div style={{ 
            padding: 16, 
            backgroundColor: '#fff3e0',
            borderRadius: 8, 
            marginBottom: 16,
            border: '1px solid #ffe0b2'
          }}>
            <h4 style={{ margin: '0 0 12px', fontSize: 14, color: '#e65100' }}>
              Outbound API Configuration (Optional)
            </h4>
            <p style={{ fontSize: 12, color: '#666', marginBottom: 12 }}>
              Send harmonized data to an external API endpoint automatically after processing.
            </p>
            
            <div style={{ marginBottom: 12 }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <input
                  type="checkbox"
                  name="outboundApiEnabled"
                  checked={formData.outboundApiEnabled}
                  onChange={handleChange}
                />
                <span style={{ fontWeight: 'bold' }}>Enable Outbound API</span>
              </label>
              <p style={{ fontSize: 11, color: '#666', margin: '4px 0 0 24px' }}>
                When enabled, all harmonized records will be sent to the configured API endpoint
              </p>
            </div>

            {formData.outboundApiEnabled && (
              <>
                <div style={{ marginBottom: 12 }}>
                  <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                    API Endpoint URL *
                  </label>
                  <input
                    type="url"
                    name="outboundApiUrl"
                    value={formData.outboundApiUrl}
                    onChange={handleChange}
                    style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                    placeholder="https://api.example.com/webhooks/data"
                    required={formData.outboundApiEnabled}
                  />
                  <p style={{ fontSize: 11, color: '#666', margin: '4px 0 0' }}>
                    POST requests with harmonized JSON will be sent to this URL
                  </p>
                </div>

                <div style={{ marginBottom: 0 }}>
                  <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
                    Custom Headers (JSON format, optional)
                  </label>
                  <textarea
                    name="outboundApiHeaders"
                    value={formData.outboundApiHeaders}
                    onChange={handleChange}
                    rows={3}
                    style={{ 
                      width: '100%', 
                      padding: 8, 
                      boxSizing: 'border-box',
                      fontFamily: 'monospace',
                      fontSize: 12
                    }}
                    placeholder={'{\n  "Authorization": "Bearer YOUR_TOKEN",\n  "X-Custom-Header": "value"\n}'}
                  />
                  <p style={{ fontSize: 11, color: '#666', margin: '4px 0 0' }}>
                    Optional: Add custom HTTP headers as JSON (e.g., authentication tokens)
                  </p>
                </div>
              </>
            )}
          </div>

          <div style={{ marginBottom: 20 }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input
                type="checkbox"
                name="isActive"
                checked={formData.isActive}
                onChange={handleChange}
              />
              <span>Active (workflow can receive webhook requests)</span>
            </label>
          </div>

          <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
            <button
              type="button"
              onClick={() => onClose(false)}
              style={{
                padding: '10px 20px',
                backgroundColor: '#f5f5f5',
                border: '1px solid #ddd',
                borderRadius: 4,
                cursor: 'pointer'
              }}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={saving}
              style={{
                padding: '10px 20px',
                backgroundColor: saving ? '#ccc' : '#007bff',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: saving ? 'default' : 'pointer'
              }}
            >
              {saving ? 'Saving...' : (workflow ? 'Update' : 'Create')} Workflow
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default WorkflowForm;
