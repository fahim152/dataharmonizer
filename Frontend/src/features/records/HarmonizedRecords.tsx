import React from 'react';
import type { HarmonizedRecord } from '../../types';

interface HarmonizedRecordsProps {
  records: HarmonizedRecord[];
  onSelect: (record: HarmonizedRecord) => void;
}

const HarmonizedRecords: React.FC<HarmonizedRecordsProps> = ({ records, onSelect }) => (
  <div style={{ width: 350, borderLeft: '1px solid #eee', overflowY: 'auto' }}>
    <h3 style={{ padding: 16 }}>Harmonized Records</h3>
    {records.length === 0 ? (
      <div style={{ padding: 16, color: '#888' }}>No harmonization done yet.</div>
    ) : (
      <ul style={{ listStyle: 'none', padding: 0 }}>
        {records.map(rec => (
          <li
            key={rec.id}
            style={{ padding: '12px 16px', cursor: 'pointer', borderBottom: '1px solid #f0f0f0' }}
            onClick={() => onSelect(rec)}
          >
            <div><b>{rec.id}</b></div>
            <div style={{ fontSize: 12, color: '#888' }}>{rec.receivedAt}</div>
          </li>
        ))}
      </ul>
    )}
  </div>
);

export default HarmonizedRecords;
