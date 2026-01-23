import React, { useState, useEffect } from 'react';
import { fetchWorkflows, deleteWorkflow } from '../../api/harmonizer';
import WorkflowForm from './WorkflowForm';
import WorkflowDetails from './WorkflowDetails';
import type { Workflow } from '../../types';

const WorkflowList: React.FC = () => {
  const [workflows, setWorkflows] = useState<Workflow[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showForm, setShowForm] = useState<boolean>(false);
  const [editingWorkflow, setEditingWorkflow] = useState<Workflow | null>(null);
  const [selectedWorkflow, setSelectedWorkflow] = useState<Workflow | null>(null);

  const loadWorkflows = async (): Promise<void> => {
    try {
      const data = await fetchWorkflows();
      setWorkflows(data);
    } catch (err) {
      console.error('Failed to load workflows:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadWorkflows();
  }, []);

  const handleDelete = async (id: number): Promise<void> => {
    if (!window.confirm('Are you sure you want to delete this workflow?')) return;
    try {
      await deleteWorkflow(id);
      setWorkflows(workflows.filter(w => w.id !== id));
      if (selectedWorkflow?.id === id) setSelectedWorkflow(null);
    } catch (err) {
      console.error('Failed to delete workflow:', err);
    }
  };

  const handleFormClose = (saved: boolean): void => {
    setShowForm(false);
    setEditingWorkflow(null);
    if (saved) loadWorkflows();
  };

  const handleEdit = (workflow: Workflow): void => {
    setEditingWorkflow(workflow);
    setShowForm(true);
  };

  if (loading) return <div style={{ padding: 24 }}>Loading workflows...</div>;

  return (
    <div style={{ display: 'flex', height: '100%', width: '100%' }}>
      {/* Workflow List Panel */}
      <div style={{ 
        width: 320, 
        borderRight: '1px solid #ddd', 
        display: 'flex', 
        flexDirection: 'column',
        backgroundColor: '#f8f9fa'
      }}>
        <div style={{ 
          padding: 16, 
          borderBottom: '1px solid #ddd',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <h3 style={{ margin: 0 }}>Workflows</h3>
          <button
            onClick={() => setShowForm(true)}
            style={{
              padding: '8px 16px',
              backgroundColor: '#28a745',
              color: 'white',
              border: 'none',
              borderRadius: 4,
              cursor: 'pointer'
            }}
          >
            + New
          </button>
        </div>
        
        <div style={{ flex: 1, overflowY: 'auto' }}>
          {workflows.length === 0 ? (
            <div style={{ padding: 16, color: '#666', textAlign: 'center' }}>
              No workflows yet. Create one to get started!
            </div>
          ) : (
            workflows.map(workflow => (
              <div
                key={workflow.id}
                onClick={() => setSelectedWorkflow(workflow)}
                style={{
                  padding: 16,
                  borderBottom: '1px solid #eee',
                  cursor: 'pointer',
                  backgroundColor: selectedWorkflow?.id === workflow.id ? '#e3f2fd' : 'transparent'
                }}
              >
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between', 
                  alignItems: 'center',
                  marginBottom: 4
                }}>
                  <span style={{ fontWeight: 'bold' }}>{workflow.name}</span>
                  <span style={{
                    fontSize: 11,
                    padding: '2px 6px',
                    borderRadius: 8,
                    backgroundColor: workflow.isActive ? '#c8e6c9' : '#ffcdd2',
                    color: workflow.isActive ? '#2e7d32' : '#c62828'
                  }}>
                    {workflow.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
                <div style={{ fontSize: 12, color: '#666' }}>
                  {workflow.description || 'No description'}
                </div>
                <div style={{ fontSize: 11, color: '#999', marginTop: 4 }}>
                  Type: {workflow.desiredOutputType}
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Workflow Details Panel */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        {selectedWorkflow ? (
          <WorkflowDetails
            workflow={selectedWorkflow}
            onEdit={() => handleEdit(selectedWorkflow)}
            onDelete={() => handleDelete(selectedWorkflow.id)}
            onRefresh={loadWorkflows}
          />
        ) : (
          <div style={{ 
            flex: 1, 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            color: '#666'
          }}>
            Select a workflow to view details
          </div>
        )}
      </div>

      {/* Workflow Form Modal */}
      {showForm && (
        <WorkflowForm
          workflow={editingWorkflow}
          onClose={handleFormClose}
        />
      )}
    </div>
  );
};

export default WorkflowList;
