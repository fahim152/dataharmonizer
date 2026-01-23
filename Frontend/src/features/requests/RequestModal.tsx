import React from 'react';
import type { HarmonizeRequest } from '../../types';

interface RequestModalProps {
  request: HarmonizeRequest;
  onClose: () => void;
}

const RequestModal: React.FC<RequestModalProps> = ({ request, onClose }) => {
  if (!request) return null;

  let formattedPayload = request.payload;
  try {
    const parsed = JSON.parse(request.payload);
    formattedPayload = JSON.stringify(parsed, null, 2);
  } catch {
    // Keep as-is if not valid JSON
  }

  return (
    <div 
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
      }}
      onClick={onClose}
    >
      <div 
        style={{
          backgroundColor: 'white',
          borderRadius: '8px',
          padding: '24px',
          maxWidth: '800px',
          maxHeight: '80vh',
          width: '90%',
          overflow: 'auto',
          boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h2 style={{ margin: 0 }}>Request Details</h2>
          <button 
            onClick={onClose}
            style={{
              background: 'none',
              border: 'none',
              fontSize: '24px',
              cursor: 'pointer',
              padding: '0 8px',
              color: '#666',
            }}
          >
            ×
          </button>
        </div>

        <div style={{ marginBottom: '20px' }}>
          <div style={{ marginBottom: '8px' }}>
            <strong>Request ID:</strong> {request.id}
          </div>
          <div style={{ marginBottom: '8px' }}>
            <strong>Source:</strong> {request.source || 'N/A'}
          </div>
          <div style={{ marginBottom: '8px' }}>
            <strong>Created At:</strong> {new Date(request.createdAt).toLocaleString()}
          </div>
        </div>

        <div>
          <h3 style={{ marginTop: '20px', marginBottom: '10px' }}>Original Payload:</h3>
          <pre 
            style={{
              backgroundColor: '#f5f5f5',
              padding: '16px',
              borderRadius: '4px',
              overflow: 'auto',
              maxHeight: '400px',
              fontSize: '12px',
              fontFamily: 'monospace',
              lineHeight: '1.5',
              border: '1px solid #ddd',
            }}
          >
            {formattedPayload}
          </pre>
        </div>

        <div style={{ marginTop: '20px', textAlign: 'right' }}>
          <button 
            onClick={onClose}
            style={{
              padding: '10px 20px',
              backgroundColor: '#007bff',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
            }}
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default RequestModal;
