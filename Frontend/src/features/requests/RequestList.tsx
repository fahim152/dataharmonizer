import React, { useState } from 'react';
import RequestModal from './RequestModal';
import type { HarmonizeRequest } from '../../types';

interface RequestListProps {
  requests: HarmonizeRequest[];
  selectedId?: number;
  onSelect: (request: HarmonizeRequest) => void;
}

const RequestList: React.FC<RequestListProps> = ({ requests, selectedId, onSelect }) => {
  const [modalRequest, setModalRequest] = useState<HarmonizeRequest | null>(null);

  const handleViewDetails = (e: React.MouseEvent, req: HarmonizeRequest): void => {
    e.stopPropagation();
    setModalRequest(req);
  };

  return (
    <>
      <div style={{ width: 280, borderRight: '1px solid #eee', overflowY: 'auto' }}>
        <h3 style={{ padding: 16 }}>Requests</h3>
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {requests.map(req => (
            <li
              key={req.id}
              style={{
                padding: '12px 16px',
                background: req.id === selectedId ? '#f0f0f0' : 'transparent',
                cursor: 'pointer',
              }}
              onClick={() => onSelect(req)}
            >
              <div>
                <div
                  onClick={(e) => handleViewDetails(e, req)}
                  onMouseEnter={(e) => (e.target as HTMLElement).style.color = '#007bff'}
                  onMouseLeave={(e) => (e.target as HTMLElement).style.color = '#000'}
                  style={{
                    fontWeight: 'bold',
                    cursor: 'pointer',
                    transition: 'color 0.2s',
                  }}
                  title="Click to view payload details"
                >
                  {req.source}
                </div>
                <div style={{ fontSize: 12, color: '#888' }}>ID: {req.id}</div>
              </div>
            </li>
          ))}
        </ul>
      </div>

      {modalRequest && (
        <RequestModal 
          request={modalRequest} 
          onClose={() => setModalRequest(null)} 
        />
      )}
    </>
  );
};

export default RequestList;
