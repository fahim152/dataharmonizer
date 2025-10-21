import React, { useState } from 'react';

const HarmonizeEditor = ({ request, onHarmonize }) => {
  const [contextPrompt, setContextPrompt] = useState('');
  const [outputType, setOutputType] = useState('text');
  const [desiredSchema, setDesiredSchema] = useState('');
  const [variables, setVariables] = useState(['']);
  const handleTypeChange = e => setOutputType(e.target.value);

  return (
    <div style={{ flex: 1, padding: 24, minWidth: 400 }}>
      <h3>Harmonize Options</h3>
      <div style={{ marginBottom: 16 }}>
        <label>Context Prompt:</label>
        <textarea value={contextPrompt} onChange={e => setContextPrompt(e.target.value)} rows={3} style={{ width: '100%' }} />
      </div>
      <div style={{ marginBottom: 16 }}>
        <label>Desired Output Type:</label>
        <select value={outputType} onChange={handleTypeChange}>
          <option value="text">Text</option>
          <option value="variable">Variable</option>
          <option value="json">JSON</option>
        </select>
      </div>
      {outputType === 'text' && (
        <div style={{ marginBottom: 16 }}>
          <label>Desired Output (Text):</label>
          <input value={desiredSchema} onChange={e => setDesiredSchema(e.target.value)} style={{ width: '100%' }} />
        </div>
      )}
      {outputType === 'variable' && (
        <div style={{ marginBottom: 16 }}>
          <label>Variables:</label>
          {variables.map((v, i) => (
            <div key={i} style={{ display: 'flex', marginBottom: 4 }}>
              <input value={v} onChange={e => {
                const arr = [...variables];
                arr[i] = e.target.value;
                setVariables(arr);
              }} style={{ flex: 1 }} />
              <button onClick={() => setVariables(variables.filter((_, idx) => idx !== i))}>-</button>
            </div>
          ))}
          <button onClick={() => setVariables([...variables, ''])}>Add Variable</button>
        </div>
      )}
      {outputType === 'json' && (
        <div style={{ marginBottom: 16 }}>
          <label>Desired Output (JSON):</label>
          <textarea value={desiredSchema} onChange={e => setDesiredSchema(e.target.value)} rows={5} style={{ width: '100%' }} />
        </div>
      )}
      <button
        disabled={!request || !contextPrompt}
        onClick={() => onHarmonize({ contextPrompt, desiredSchema: outputType === 'variable' ? variables : desiredSchema, outputType })}
      >
        Harmonize
      </button>
    </div>
  );
};

export default HarmonizeEditor;

