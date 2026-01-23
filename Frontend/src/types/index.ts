// Type definitions for the Harmonizer API

export interface HarmonizeRequest {
  id: number;
  payload: string;
  source: string | null;
  createdAt: string;
  workflowId?: number | null;
}

export interface HarmonizedRecord {
  id: number;
  rawJson: string;
  harmonizedJson: string | null;
  receivedAt: string;
  source: string | null;
  harmonizeRequestId: number;
}

export interface Prompt {
  id: number;
  name: string;
  description: string | null;
  contextPrompt: string;
  desiredOutput: string;
  desiredOutputType: string;
  createdAt: string;
}

export interface Workflow {
  id: number;
  name: string;
  description: string;
  contextPrompt: string;
  desiredOutput: string;
  desiredOutputType: string;
  apiKey: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
  teamId?: number | null;
  entityName?: string | null;
  outboundApiEnabled: boolean;
  outboundApiUrl?: string | null;
  outboundApiHeaders?: string | null;
}

export interface WorkflowCreateDto {
  name: string;
  description: string;
  contextPrompt: string;
  desiredOutput: string;
  desiredOutputType: string;
  teamId?: number | null;
  entityName?: string | null;
  inputConfig?: InputConfigCreateDto;
  outputConfig?: OutputConfigCreateDto;
}

export interface WorkflowUpdateDto {
  name?: string;
  description?: string;
  contextPrompt?: string;
  desiredOutput?: string;
  desiredOutputType?: string;
  isActive?: boolean;
  teamId?: number | null;
  entityName?: string | null;
  outboundApiEnabled?: boolean;
  outboundApiUrl?: string | null;
  outboundApiHeaders?: string | null;
}

export interface PromptCreateDto {
  name: string;
  description?: string;
  contextPrompt: string;
  desiredOutput: string;
  desiredOutputType: string;
}

export interface HarmonizeOptions {
  contextPrompt: string;
  desiredSchema: string | string[];
  outputType: string;
}

// Multi-tenancy types
export interface Tenant {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  namespaceCount: number;
}

export interface Namespace {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  tenantId: number;
  tenantName: string;
  teamCount: number;
}

export interface Team {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  namespaceId: number;
  namespaceName: string;
  tenantName: string;
  workflowCount: number;
  fullPath: string;
}

export interface TenantHierarchy {
  id: number;
  name: string;
  slug: string;
  namespaces: NamespaceHierarchy[];
}

export interface NamespaceHierarchy {
  id: number;
  name: string;
  slug: string;
  teams: TeamHierarchy[];
}

export interface TeamHierarchy {
  id: number;
  name: string;
  slug: string;
}

// Workflow configuration types
export type InputSourceType = 'Manual' | 'Webhook' | 'ApiPoll' | 'FileUpload';
export type OutputDestinationType = 'Database' | 'Api' | 'Both';
export type AuthType = 'None' | 'ApiKey' | 'Bearer' | 'Basic';

export interface InputConfig {
  id: number;
  sourceType: InputSourceType;
  pollUrl?: string;
  pollIntervalSeconds?: number;
  pollHttpMethod?: string;
  pollHeaders?: string;
  pollBody?: string;
  authType: AuthType;
  isPollingEnabled: boolean;
  lastPollAt?: string;
}

export interface InputConfigCreateDto {
  sourceType: InputSourceType;
  pollUrl?: string;
  pollIntervalSeconds?: number;
  pollHttpMethod?: string;
  pollHeaders?: string;
  pollBody?: string;
  authType?: AuthType;
  authCredentials?: string;
  isPollingEnabled?: boolean;
}

export interface OutputConfig {
  id: number;
  destinationType: OutputDestinationType;
  apiEndpointUrl?: string;
  apiHttpMethod?: string;
  apiHeaders?: string;
  apiAuthType: AuthType;
  customTableName?: string;
  autoCreateTable: boolean;
  tableSchema?: string;
  hasCustomConnection: boolean;
  generatedTableName?: string;
}

export interface OutputConfigCreateDto {
  destinationType: OutputDestinationType;
  apiEndpointUrl?: string;
  apiHttpMethod?: string;
  apiHeaders?: string;
  apiAuthType?: AuthType;
  apiAuthCredentials?: string;
  customTableName?: string;
  autoCreateTable?: boolean;
  customConnectionString?: string;
}

export interface WorkflowFull extends Workflow {
  teamName?: string;
  namespaceName?: string;
  tenantName?: string;
  inputConfig?: InputConfig;
  outputConfig?: OutputConfig;
  webhookUrl: string;
  generatedTableName?: string;
  requestCount: number;
}

