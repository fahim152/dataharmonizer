import React from 'react';
import type { HarmonizedRecord } from '../../types';

interface RecordModalProps {
  record: HarmonizedRecord;
  onClose: () => void;
}

const RecordModal: React.FC<RecordModalProps> = ({ record, onClose }) => (
  <div style={{
    position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
    background: 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
  }}>
    <div style={{ background: '#fff', padding: 32, borderRadius: 8, minWidth: 400, maxWidth: 700 }}>
      <h2>Harmonized Record #{record.id}</h2>
      <pre style={{ background: '#f7f7f7', padding: 16, borderRadius: 4, maxHeight: 400, overflow: 'auto' }}>
        {record.harmonizedJson}
      </pre>
      <button onClick={onClose} style={{ marginTop: 16 }}>Close</button>
    </div>
  </div>
);

export default RecordModal;
