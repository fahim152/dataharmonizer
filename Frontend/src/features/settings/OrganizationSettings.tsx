import React, { useState, useEffect } from 'react';
import { 
  fetchTenantHierarchy, 
  createTenant, 
  createNamespace, 
  createTeam,
  deleteTenant,
  deleteNamespace,
  deleteTeam
} from '../../api/harmonizer';
import type { TenantHierarchy } from '../../types';

const OrganizationSettings: React.FC = () => {
  const [hierarchy, setHierarchy] = useState<TenantHierarchy[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState<'tenant' | 'namespace' | 'team' | null>(null);
  const [selectedParent, setSelectedParent] = useState<{ tenantId?: number; namespaceId?: number }>({});
  const [formData, setFormData] = useState({ name: '', description: '' });
  const [saving, setSaving] = useState(false);

  const loadHierarchy = async () => {
    try {
      const data = await fetchTenantHierarchy();
      setHierarchy(data);
    } catch (err) {
      console.error('Failed to load hierarchy:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadHierarchy();
  }, []);

  const handleAdd = async () => {
    if (!formData.name.trim()) return;
    setSaving(true);
    
    try {
      if (showAddModal === 'tenant') {
        await createTenant({ name: formData.name, description: formData.description });
      } else if (showAddModal === 'namespace' && selectedParent.tenantId) {
        await createNamespace({ 
          name: formData.name, 
          description: formData.description,
          tenantId: selectedParent.tenantId 
        });
      } else if (showAddModal === 'team' && selectedParent.namespaceId) {
        await createTeam({ 
          name: formData.name, 
          description: formData.description,
          namespaceId: selectedParent.namespaceId 
        });
      }
      
      setShowAddModal(null);
      setFormData({ name: '', description: '' });
      loadHierarchy();
    } catch (err) {
      console.error('Failed to add:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (type: 'tenant' | 'namespace' | 'team', id: number, name: string) => {
    if (!window.confirm(`Delete "${name}"? This will also delete all nested items.`)) return;
    
    try {
      if (type === 'tenant') await deleteTenant(id);
      else if (type === 'namespace') await deleteNamespace(id);
      else await deleteTeam(id);
      loadHierarchy();
    } catch (err) {
      console.error('Failed to delete:', err);
    }
  };

  if (loading) return <div style={{ padding: 24 }}>Loading organization structure...</div>;

  return (
    <div style={{ padding: 24, maxWidth: 900, margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <h2 style={{ margin: 0 }}>Organization Structure</h2>
          <p style={{ color: '#666', margin: '4px 0 0' }}>
            Manage tenants, namespaces, and teams for multi-tenant data isolation
          </p>
        </div>
        <button
          onClick={() => setShowAddModal('tenant')}
          style={{
            padding: '10px 20px',
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            borderRadius: 4,
            cursor: 'pointer'
          }}
        >
          + Add Tenant
        </button>
      </div>

      {/* Hierarchy Display */}
      <div style={{ backgroundColor: '#f8f9fa', borderRadius: 8, padding: 16 }}>
        {hierarchy.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 40, color: '#666' }}>
            <p>No tenants configured yet.</p>
            <p>Add a tenant to get started with multi-tenancy.</p>
          </div>
        ) : (
          hierarchy.map(tenant => (
            <div key={tenant.id} style={{ marginBottom: 16 }}>
              {/* Tenant Row */}
              <div style={{ 
                display: 'flex', 
                alignItems: 'center', 
                padding: '12px 16px',
                backgroundColor: '#1a1a2e',
                color: 'white',
                borderRadius: '8px 8px 0 0',
                gap: 12
              }}>
                <span style={{ fontWeight: 'bold', flex: 1 }}>{tenant.name}</span>
                <span style={{ fontSize: 12, opacity: 0.7 }}>/{tenant.slug}</span>
                <button
                  onClick={() => {
                    setSelectedParent({ tenantId: tenant.id });
                    setShowAddModal('namespace');
                  }}
                  style={{
                    padding: '4px 12px',
                    backgroundColor: '#007bff',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 12
                  }}
                >
                  + Namespace
                </button>
                <button
                  onClick={() => handleDelete('tenant', tenant.id, tenant.name)}
                  style={{
                    padding: '4px 8px',
                    backgroundColor: 'transparent',
                    color: '#ff6b6b',
                    border: '1px solid #ff6b6b',
                    borderRadius: 4,
                    cursor: 'pointer',
                    fontSize: 12
                  }}
                >
                  Delete
                </button>
              </div>

              {/* Namespaces */}
              <div style={{ 
                backgroundColor: 'white', 
                border: '1px solid #ddd',
                borderTop: 'none',
                borderRadius: '0 0 8px 8px'
              }}>
                {tenant.namespaces.length === 0 ? (
                  <div style={{ padding: 16, color: '#888', fontStyle: 'italic' }}>
                    No namespaces yet
                  </div>
                ) : (
                  tenant.namespaces.map(ns => (
                    <div key={ns.id}>
                      {/* Namespace Row */}
                      <div style={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        padding: '10px 16px 10px 32px',
                        borderBottom: '1px solid #eee',
                        gap: 12
                      }}>
                        <span style={{ fontWeight: 500, flex: 1 }}>{ns.name}</span>
                        <span style={{ fontSize: 12, color: '#888' }}>/{ns.slug}</span>
                        <button
                          onClick={() => {
                            setSelectedParent({ namespaceId: ns.id });
                            setShowAddModal('team');
                          }}
                          style={{
                            padding: '3px 10px',
                            backgroundColor: '#17a2b8',
                            color: 'white',
                            border: 'none',
                            borderRadius: 4,
                            cursor: 'pointer',
                            fontSize: 11
                          }}
                        >
                          + Team
                        </button>
                        <button
                          onClick={() => handleDelete('namespace', ns.id, ns.name)}
                          style={{
                            padding: '3px 6px',
                            backgroundColor: 'transparent',
                            color: '#dc3545',
                            border: 'none',
                            cursor: 'pointer',
                            fontSize: 14
                          }}
                        >
                          ✕
                        </button>
                      </div>

                      {/* Teams */}
                      {ns.teams.map(team => (
                        <div 
                          key={team.id}
                          style={{ 
                            display: 'flex', 
                            alignItems: 'center', 
                            padding: '8px 16px 8px 56px',
                            borderBottom: '1px solid #f0f0f0',
                            gap: 12
                          }}
                        >
                          <span style={{ flex: 1 }}>{team.name}</span>
                          <span style={{ fontSize: 11, color: '#aaa' }}>/{team.slug}</span>
                          <button
                            onClick={() => handleDelete('team', team.id, team.name)}
                            style={{
                              padding: '2px 6px',
                              backgroundColor: 'transparent',
                              color: '#999',
                              border: 'none',
                              cursor: 'pointer',
                              fontSize: 12
                            }}
                          >
                            ✕
                          </button>
                        </div>
                      ))}
                    </div>
                  ))
                )}
              </div>
            </div>
          ))
        )}
      </div>

      {/* Table naming convention info */}
      <div style={{ 
        marginTop: 24, 
        padding: 16, 
        backgroundColor: '#e8f4fd', 
        borderRadius: 8,
        border: '1px solid #b8daff'
      }}>
        <h4 style={{ margin: '0 0 8px' }}>Dynamic Table Naming</h4>
        <p style={{ margin: 0, fontSize: 14, color: '#004085' }}>
          When a workflow is assigned to a team, output tables are named:
          <code style={{ 
            backgroundColor: '#fff', 
            padding: '2px 8px', 
            borderRadius: 4, 
            marginLeft: 8 
          }}>
            {'{tenant}_{namespace}_{team}_{entity}'}
          </code>
        </p>
        <p style={{ margin: '8px 0 0', fontSize: 13, color: '#666' }}>
          Example: <code>acme_production_iot_sensor_readings</code>
        </p>
      </div>

      {/* Add Modal */}
      {showAddModal && (
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
            padding: 24,
            width: 400
          }}>
            <h3 style={{ margin: '0 0 16px' }}>
              Add {showAddModal.charAt(0).toUpperCase() + showAddModal.slice(1)}
            </h3>
            
            <div style={{ marginBottom: 16 }}>
              <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>Name</label>
              <input
                type="text"
                value={formData.name}
                onChange={e => setFormData({ ...formData, name: e.target.value })}
                style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                placeholder={`Enter ${showAddModal} name`}
                autoFocus
              />
            </div>
            
            <div style={{ marginBottom: 16 }}>
              <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>Description</label>
              <input
                type="text"
                value={formData.description}
                onChange={e => setFormData({ ...formData, description: e.target.value })}
                style={{ width: '100%', padding: 8, boxSizing: 'border-box' }}
                placeholder="Optional description"
              />
            </div>
            
            <div style={{ display: 'flex', gap: 12, justifyContent: 'flex-end' }}>
              <button
                onClick={() => {
                  setShowAddModal(null);
                  setFormData({ name: '', description: '' });
                }}
                style={{
                  padding: '8px 16px',
                  backgroundColor: '#f5f5f5',
                  border: '1px solid #ddd',
                  borderRadius: 4,
                  cursor: 'pointer'
                }}
              >
                Cancel
              </button>
              <button
                onClick={handleAdd}
                disabled={saving || !formData.name.trim()}
                style={{
                  padding: '8px 16px',
                  backgroundColor: saving ? '#ccc' : '#007bff',
                  color: 'white',
                  border: 'none',
                  borderRadius: 4,
                  cursor: saving ? 'default' : 'pointer'
                }}
              >
                {saving ? 'Adding...' : 'Add'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrganizationSettings;
