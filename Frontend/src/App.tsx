import React, { useEffect, useState } from 'react';
import RequestList from './features/requests/RequestList';
import HarmonizeEditor from './features/harmonization/HarmonizeEditor';
import HarmonizedRecords from './features/records/HarmonizedRecords';
import RecordModal from './features/records/RecordModal';
import WorkflowList from './features/workflows/WorkflowList';
import ManualDataEntry from './features/ingest/ManualDataEntry';
import OrganizationSettings from './features/settings/OrganizationSettings';
import { fetchRequests, fetchRecords, harmonizeRequest } from './api/harmonizer';
import type { HarmonizeRequest, HarmonizedRecord, HarmonizeOptions } from './types';

type TabType = 'ingest' | 'harmonize' | 'workflows' | 'settings';

const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabType>('ingest');
  const [requests, setRequests] = useState<HarmonizeRequest[]>([]);
  const [selectedRequest, setSelectedRequest] = useState<HarmonizeRequest | null>(null);
  const [records, setRecords] = useState<HarmonizedRecord[]>([]);
  const [modalRecord, setModalRecord] = useState<HarmonizedRecord | null>(null);

  useEffect(() => {
    if (activeTab === 'harmonize') {
      fetchRequests().then(setRequests);
    }
  }, [activeTab]);

  useEffect(() => {
    if (selectedRequest) {
      fetchRecords(selectedRequest.id).then(setRecords);
    } else {
      setRecords([]);
    }
  }, [selectedRequest]);

  const handleHarmonize = async ({ contextPrompt, desiredSchema }: HarmonizeOptions): Promise<void> => {
    if (!selectedRequest) return;
    await harmonizeRequest(selectedRequest.id, contextPrompt, desiredSchema);
    fetchRecords(selectedRequest.id).then(setRecords);
  };

  const navItems: { key: TabType; label: string }[] = [
    { key: 'ingest', label: 'Data Entry' },
    { key: 'workflows', label: 'Workflows' },
    { key: 'harmonize', label: 'Requests' },
    { key: 'settings', label: 'Settings' },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      {/* Header */}
      <header style={{
        backgroundColor: '#1a1a2e',
        padding: '12px 24px',
        display: 'flex',
        alignItems: 'center',
        gap: 16
      }}>
        <h1 style={{ 
          margin: 0, 
          fontSize: 20, 
          color: '#fff',
          display: 'flex',
          alignItems: 'center',
          gap: 8
        }}>
          Data Harmonizer
        </h1>
        <span style={{ 
          backgroundColor: '#007bff', 
          color: 'white', 
          padding: '2px 8px', 
          borderRadius: 4,
          fontSize: 11
        }}>
          v2.0
        </span>
      </header>

      {/* Navigation Tabs */}
      <nav style={{
        display: 'flex',
        backgroundColor: '#16213e',
        padding: '0 16px',
        gap: 4,
        borderBottom: '1px solid #0f3460'
      }}>
        {navItems.map(item => (
          <button
            key={item.key}
            onClick={() => setActiveTab(item.key)}
            style={{
              padding: '14px 20px',
              backgroundColor: activeTab === item.key ? '#0f3460' : 'transparent',
              color: activeTab === item.key ? '#fff' : '#888',
              border: 'none',
              borderBottom: activeTab === item.key ? '3px solid #007bff' : '3px solid transparent',
              cursor: 'pointer',
              fontSize: 14,
              fontWeight: activeTab === item.key ? 'bold' : 'normal',
              transition: 'all 0.2s'
            }}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {/* Main Content */}
      <div style={{ flex: 1, display: 'flex', overflow: 'hidden', backgroundColor: '#f5f5f5' }}>
        {activeTab === 'ingest' && (
          <div style={{ flex: 1, overflow: 'auto' }}>
            <ManualDataEntry />
          </div>
        )}
        
        {activeTab === 'harmonize' && (
          <>
            <RequestList
              requests={requests}
              selectedId={selectedRequest?.id}
              onSelect={setSelectedRequest}
            />
            <HarmonizeEditor
              request={selectedRequest}
              onHarmonize={handleHarmonize}
            />
            <HarmonizedRecords
              records={records}
              onSelect={setModalRecord}
            />
            {modalRecord && (
              <RecordModal record={modalRecord} onClose={() => setModalRecord(null)} />
            )}
          </>
        )}
        
        {activeTab === 'workflows' && (
          <WorkflowList />
        )}
        
        {activeTab === 'settings' && (
          <div style={{ flex: 1, overflow: 'auto' }}>
            <OrganizationSettings />
          </div>
        )}
      </div>
    </div>
  );
};

export default App;
