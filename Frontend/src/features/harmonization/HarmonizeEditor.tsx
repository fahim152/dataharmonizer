import React, { useState, useEffect } from 'react';
import { fetchPrompts } from '../../api/harmonizer';
import type { HarmonizeRequest, Prompt, HarmonizeOptions } from '../../types';

interface HarmonizeEditorProps {
  request: HarmonizeRequest | null;
  onHarmonize: (options: HarmonizeOptions) => Promise<void>;
}

const HarmonizeEditor: React.FC<HarmonizeEditorProps> = ({ request, onHarmonize }) => {
  const [contextPrompt, setContextPrompt] = useState<string>('');
  const [outputType, setOutputType] = useState<string>('text');
  const [desiredSchema, setDesiredSchema] = useState<string>('');
  const [variables, setVariables] = useState<string[]>(['']);
  const [prompts, setPrompts] = useState<Prompt[]>([]);
  const [selectedPromptId, setSelectedPromptId] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    async function loadPrompts(): Promise<void> {
      try {
        const data = await fetchPrompts();
        setPrompts(data);
      } catch (err) {
        console.error('Failed to load prompts:', err);
      } finally {
        setLoading(false);
      }
    }
    loadPrompts();
  }, []);

  const handlePromptSelect = (e: React.ChangeEvent<HTMLSelectElement>): void => {
    const promptId = e.target.value;
    setSelectedPromptId(promptId);
    
    if (promptId) {
      const selected = prompts.find(p => p.id === parseInt(promptId));
      if (selected) {
        setContextPrompt(selected.contextPrompt);
        setDesiredSchema(selected.desiredOutput);
        
        const type = selected.desiredOutputType?.toLowerCase() || 'text';
        if (type === 'json') {
          setOutputType('json');
        } else if (type === 'variable') {
          setOutputType('variable');
        } else {
          setOutputType('text');
        }
      }
    }
  };

  const handleTypeChange = (e: React.ChangeEvent<HTMLSelectElement>): void => {
    setOutputType(e.target.value);
  };

  return (
    <div style={{ flex: 1, padding: 24, minWidth: 400 }}>
      <h3>Harmonize Options</h3>
      
      {/* Prompt Template Selector */}
      <div style={{ marginBottom: 16 }}>
        <label style={{ display: 'block', marginBottom: 4, fontWeight: 'bold' }}>
          Select Prompt Template:
        </label>
        {loading ? (
          <p>Loading prompts...</p>
        ) : (
          <select 
            value={selectedPromptId} 
            onChange={handlePromptSelect}
            style={{ width: '100%', padding: '8px', marginBottom: '8px' }}
          >
            <option value="">-- Select a template --</option>
            {prompts.map(prompt => (
              <option key={prompt.id} value={prompt.id}>
                {prompt.name}
              </option>
            ))}
          </select>
        )}
        {selectedPromptId && (
          <p style={{ fontSize: '12px', color: '#666', marginTop: '4px' }}>
            {prompts.find(p => p.id === parseInt(selectedPromptId))?.description}
          </p>
        )}
      </div>

      <div style={{ marginBottom: 16 }}>
        <label>Context Prompt:</label>
        <textarea 
          value={contextPrompt} 
          onChange={e => setContextPrompt(e.target.value)} 
          rows={3} 
          style={{ width: '100%' }} 
          placeholder="Enter harmonization instructions or select a template above"
        />
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
          <input 
            value={desiredSchema} 
            onChange={e => setDesiredSchema(e.target.value)} 
            style={{ width: '100%' }} 
          />
        </div>
      )}
      
      {outputType === 'variable' && (
        <div style={{ marginBottom: 16 }}>
          <label>Variables:</label>
          {variables.map((v, i) => (
            <div key={i} style={{ display: 'flex', marginBottom: 4 }}>
              <input 
                value={v} 
                onChange={e => {
                  const arr = [...variables];
                  arr[i] = e.target.value;
                  setVariables(arr);
                }} 
                style={{ flex: 1 }} 
              />
              <button onClick={() => setVariables(variables.filter((_, idx) => idx !== i))}>-</button>
            </div>
          ))}
          <button onClick={() => setVariables([...variables, ''])}>Add Variable</button>
        </div>
      )}
      
      {outputType === 'json' && (
        <div style={{ marginBottom: 16 }}>
          <label>Desired Output (JSON):</label>
          <textarea 
            value={desiredSchema} 
            onChange={e => setDesiredSchema(e.target.value)} 
            rows={5} 
            style={{ width: '100%', fontFamily: 'monospace' }} 
            placeholder="Enter JSON schema for desired output"
          />
        </div>
      )}
      
      <button
        disabled={!request || !contextPrompt}
        onClick={() => onHarmonize({ 
          contextPrompt, 
          desiredSchema: outputType === 'variable' ? variables : desiredSchema, 
          outputType 
        })}
        style={{ 
          padding: '10px 20px', 
          backgroundColor: (!request || !contextPrompt) ? '#ccc' : '#007bff',
          color: 'white',
          border: 'none',
          borderRadius: '4px',
          cursor: (!request || !contextPrompt) ? 'not-allowed' : 'pointer'
        }}
      >
        Harmonize
      </button>
    </div>
  );
};

export default HarmonizeEditor;
