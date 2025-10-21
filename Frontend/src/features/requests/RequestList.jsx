import React from 'react';

const RequestList = ({ requests, selectedId, onSelect }) => (
  <div style={{ width: 280, borderRight: '1px solid #eee', overflowY: 'auto' }}>
    <h3 style={{ padding: 16 }}>Requests</h3>
    <ul style={{ listStyle: 'none', padding: 0 }}>
      {requests.map(req => (
        <li
          key={req.id}
          style={{
            padding: '12px 16px',
            background: req.id === selectedId ? '#f0f0f0' : 'transparent',
            cursor: 'pointer'
          }}
          onClick={() => onSelect(req)}
        >
          <div><b>{req.source}</b></div>
          <div style={{ fontSize: 12, color: '#888' }}>{req.id}</div>
        </li>
      ))}
    </ul>
  </div>
);

export default RequestList;

