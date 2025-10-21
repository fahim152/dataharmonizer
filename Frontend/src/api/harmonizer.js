const API_BASE = import.meta.env.VITE_API_BASE_URL || '';

// API calls for harmonize requests
export async function fetchRequests() {
  // Update the URL if your backend is hosted elsewhere
  const res = await fetch(`${API_BASE}/api/HarmonizeRequests`);
  return res.json();
}

export async function fetchRecords(requestId) {
  // Update the URL if your backend is hosted elsewhere
  const res = await fetch(`${API_BASE}/api/HarmonizedRecords/by-request/${requestId}`);
  return res.json();
}

export async function harmonizeRequest(requestId, contextPrompt, desiredSchema) {
  // Update the URL if your backend is hosted elsewhere
  const res = await fetch(`${API_BASE}/api/HarmonizedRecords/harmonize/${requestId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ contextPrompt, desiredSchema })
  });
  return res.json();
}
