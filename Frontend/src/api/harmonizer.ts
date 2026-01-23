import type { 
  HarmonizeRequest, 
  HarmonizedRecord, 
  Prompt, 
  Workflow, 
  WorkflowCreateDto, 
  WorkflowUpdateDto,
  PromptCreateDto 
} from '../types';

const API_BASE = import.meta.env.VITE_API_BASE_URL || '';

// API calls for harmonize requests
export async function fetchRequests(): Promise<HarmonizeRequest[]> {
  const res = await fetch(`${API_BASE}/api/HarmonizeRequests`);
  return res.json();
}

export async function fetchRequestsByWorkflow(workflowId: number): Promise<HarmonizeRequest[]> {
  const res = await fetch(`${API_BASE}/api/HarmonizeRequests/by-workflow/${workflowId}`);
  return res.json();
}

export async function fetchRecords(requestId: number): Promise<HarmonizedRecord[]> {
  const res = await fetch(`${API_BASE}/api/HarmonizedRecords/by-request/${requestId}`);
  return res.json();
}

export async function fetchRecordsByWorkflow(workflowId: number): Promise<HarmonizedRecord[]> {
  const res = await fetch(`${API_BASE}/api/HarmonizedRecords/by-workflow/${workflowId}`);
  return res.json();
}

export async function harmonizeRequest(
  requestId: number, 
  contextPrompt: string, 
  desiredSchema: string | string[]
): Promise<HarmonizedRecord> {
  const res = await fetch(`${API_BASE}/api/HarmonizedRecords/harmonize/${requestId}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ contextPrompt, desiredSchema })
  });
  return res.json();
}

// API calls for prompts
export async function fetchPrompts(): Promise<Prompt[]> {
  const res = await fetch(`${API_BASE}/api/Prompts`);
  return res.json();
}

export async function fetchPrompt(id: number): Promise<Prompt> {
  const res = await fetch(`${API_BASE}/api/Prompts/${id}`);
  return res.json();
}

export async function createPrompt(promptData: PromptCreateDto): Promise<Prompt> {
  const res = await fetch(`${API_BASE}/api/Prompts`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(promptData)
  });
  return res.json();
}

export async function updatePrompt(id: number, promptData: Partial<PromptCreateDto>): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Prompts/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(promptData)
  });
  return res.ok;
}

export async function deletePrompt(id: number): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Prompts/${id}`, {
    method: 'DELETE'
  });
  return res.ok;
}

// API calls for workflows
export async function fetchWorkflows(): Promise<Workflow[]> {
  const res = await fetch(`${API_BASE}/api/Workflows`);
  return res.json();
}

export async function fetchWorkflow(id: number): Promise<Workflow> {
  const res = await fetch(`${API_BASE}/api/Workflows/${id}`);
  return res.json();
}

export async function createWorkflow(workflowData: WorkflowCreateDto): Promise<Workflow> {
  const res = await fetch(`${API_BASE}/api/Workflows`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(workflowData)
  });
  return res.json();
}

export async function updateWorkflow(id: number, workflowData: WorkflowUpdateDto): Promise<Workflow> {
  const res = await fetch(`${API_BASE}/api/Workflows/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(workflowData)
  });
  if (!res.ok) throw new Error('Failed to update workflow');
  return res.json();
}

export async function deleteWorkflow(id: number): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Workflows/${id}`, {
    method: 'DELETE'
  });
  return res.ok;
}

export async function regenerateWorkflowApiKey(workflowId: number): Promise<{ apiKey: string }> {
  const res = await fetch(`${API_BASE}/api/Workflows/${workflowId}/regenerate-key`, {
    method: 'POST'
  });
  return res.json();
}

export async function getWorkflowTableName(workflowId: number): Promise<{ tableName: string | null; exists: boolean }> {
  const res = await fetch(`${API_BASE}/api/Workflows/${workflowId}/table-name`);
  return res.json();
}

export function getWorkflowWebhookUrl(apiKey: string): string {
  return `${API_BASE}/api/Workflows/webhook/${apiKey}`;
}

// Multi-tenancy API calls
import type { Tenant, Namespace, Team, TenantHierarchy } from '../types';

export async function fetchTenants(): Promise<Tenant[]> {
  const res = await fetch(`${API_BASE}/api/Tenants`);
  return res.json();
}

export async function fetchTenantHierarchy(): Promise<TenantHierarchy[]> {
  const res = await fetch(`${API_BASE}/api/Tenants/hierarchy`);
  return res.json();
}

export async function createTenant(data: { name: string; slug?: string; description?: string }): Promise<Tenant> {
  const res = await fetch(`${API_BASE}/api/Tenants`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return res.json();
}

export async function deleteTenant(id: number): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Tenants/${id}`, { method: 'DELETE' });
  return res.ok;
}

export async function fetchNamespaces(tenantId?: number): Promise<Namespace[]> {
  const url = tenantId 
    ? `${API_BASE}/api/Namespaces?tenantId=${tenantId}`
    : `${API_BASE}/api/Namespaces`;
  const res = await fetch(url);
  return res.json();
}

export async function createNamespace(data: { name: string; slug?: string; description?: string; tenantId: number }): Promise<Namespace> {
  const res = await fetch(`${API_BASE}/api/Namespaces`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return res.json();
}

export async function deleteNamespace(id: number): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Namespaces/${id}`, { method: 'DELETE' });
  return res.ok;
}

export async function fetchTeams(namespaceId?: number): Promise<Team[]> {
  const url = namespaceId 
    ? `${API_BASE}/api/Teams?namespaceId=${namespaceId}`
    : `${API_BASE}/api/Teams`;
  const res = await fetch(url);
  return res.json();
}

export async function createTeam(data: { name: string; slug?: string; description?: string; namespaceId: number }): Promise<Team> {
  const res = await fetch(`${API_BASE}/api/Teams`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  return res.json();
}

export async function deleteTeam(id: number): Promise<boolean> {
  const res = await fetch(`${API_BASE}/api/Teams/${id}`, { method: 'DELETE' });
  return res.ok;
}

