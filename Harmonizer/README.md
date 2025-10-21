# Harmonizer
// ...existing content...
## LLM Configuration
Set LM Studio details in `appsettings.json` under `LLM`:
```
"LLM": {
  "BaseUrl": "http://192.168.32.5:9900",
  "Model": "lmstudio-community/llama-3.1-8b-instruct",
  "Temperature": 0.2,
  "MaxTokens": 2048
}
```
Adjust the `Model` to match one available in your LM Studio instance.

## New Endpoints
- `GET /api/HarmonizedRecords/harmonized` : list only records that have been harmonized.
- `POST /api/HarmonizedRecords/harmonize/{recordId}` : harmonize an existing record using the configured LLM.

The service calls LM Studio's OpenAI-compatible `/v1/chat/completions` endpoint.

## Simplified Harmonize Endpoint
The harmonize API now accepts ONLY a raw JSON payload as a string and creates a new record with harmonized output:

```
POST /api/HarmonizedRecords/harmonize
```

Body example (Content-Type: application/json):
```
"{ \n  \"projId\": \"PRJ-2025-001\", \n  \"title\": \"Next Gen Platform Revamp\" \n}"
```

Note: The body is a JSON string literal containing the raw JSON to harmonize. The service validates the string is valid JSON, sends it to the LLM, stores both raw and harmonized versions, and returns the harmonization response.

## Create Record Input Formats
You may send either:
1. Object form:
```
{
  "payloadObject": { "projId": "PRJ-2025-001", "title": "Next Gen Platform Revamp" },
  "source": "ProjectSystemX"
}
```
2. String form:
```
{
  "payload": "{\"projId\":\"PRJ-2025-002\",\"title\":\"AI Data Lake\"}",
  "source": "ProjectSystemX"
}
```
Object form is preferred (no escaping). String form maintained for backward compatibility.

## Data Flow (Revised)
1. External system sends a raw JSON string inside `payload` when creating a record:
```
POST /api/HarmonizedRecords
{
  "payload": "{\"projId\":\"PRJ-2025-001\",\"title\":\"Next Gen Platform Revamp\"}",
  "source": "SystemA"
}
```
2. Later, to harmonize that stored record, call:
```
POST /api/HarmonizedRecords/harmonize/{recordId}
{
  "contextPrompt": "Generate concise project report ...",
  "desiredSchema": { "report": { "projectId": "", "title": "", "progressPercent": 0 } }
}
```
3. Retrieve harmonized data via:
```
GET /api/HarmonizedRecords/{id}
GET /api/HarmonizedRecords/harmonized
```

`contextPrompt` guides the LLM; `desiredSchema` is optional and provides a structural hint.
