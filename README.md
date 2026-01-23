# Data Harmonizer Platform

> **AI-Powered Data Integration & Semantic Mapping System**

Transform diverse data sources into unified formats using intelligent LLM-based harmonization. Built for enterprise multi-tenancy with automatic schema inference and dynamic table creation.

## Overview

Data Harmonizer eliminates manual data cleaning by automatically harmonizing data from multiple sources into standardized formats. Perfect for IoT platforms, manufacturing systems, quality control, and any scenario requiring semantic data mapping at scale.

### Key Features

- **Multi-Tenant Architecture**: Complete organizational isolation (Tenant > Namespace > Team)
- **AI-Powered Transformation**: LLM-based semantic mapping with custom prompts
- **Dynamic Table Creation**: Auto-generate database schemas from JSON output
- **Flexible Sources**: Webhooks, manual entry, API polling
- **Multiple Destinations**: Database tables, external APIs, or both
- **Real-Time Pipeline View**: Visualize data flow from inbound to outbound
- **Reusable Templates**: Save and share prompt patterns

## Quick Start

### Prerequisites

- **.NET 8 SDK**: [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+**: [Download](https://nodejs.org/)
- **LM Studio** (optional): [Download](https://lmstudio.ai/)

### Installation

```bash
# Clone repository
git clone <repository-url>
cd dataharmonizer

# Backend setup
cd Harmonizer
dotnet restore
dotnet build
dotnet run
# Backend runs at http://localhost:5144

# Frontend setup (new terminal)
cd Frontend
npm install
npm run dev
# Frontend runs at http://localhost:3000
```

### First Steps

The application auto-seeds with demo data including:
- Sample tenant: **Acme Corporation**
- Namespaces: **Production**, **Staging**
- Teams: **IoT Platform**, **Manufacturing**, **Quality Control**
- 3 pre-configured workflows with team assignments

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│               DATA HARMONIZATION PIPELINE                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  INBOUND          HARMONIZE          OUTBOUND               │
│  ┌──────────┐    ┌──────────┐      ┌──────────┐           │
│  │ Webhook  │───>│   LLM    │─────>│ Database │           │
│  │ Manual   │    │ Context  │      │ API Fwd  │           │
│  │ API Poll │    │ Prompts  │      │ Both     │           │
│  └──────────┘    └──────────┘      └──────────┘           │
│                                                              │
│  Multi-Tenant Tables:                                       │
│  {tenant}_{namespace}_{team}_{entity}                       │
│  Example: acme_production_iot_sensor_readings               │
└─────────────────────────────────────────────────────────────┘
```

## Usage Guide

### Creating a Workflow

1. **Organization Setup** (Settings Tab)
   - Create Tenant (e.g., "Acme Corp")
   - Add Namespace (e.g., "Production")
   - Add Team (e.g., "IoT Platform")

2. **Workflow Creation** (Workflows Tab)
   - Click "Create Workflow"
   - Enter name and description
   - Write or select prompt template
   - Define desired output schema (JSON)
   - **Outbound Configuration**:
     - Select team from dropdown
     - Enter entity name (e.g., "sensor_readings")
     - View generated table name preview

3. **Data Ingestion**
   - **Option A**: Copy webhook URL, configure external system
   - **Option B**: Use "Data Entry" tab for manual testing

4. **Monitor Pipeline** (Workflow Details)
   - View data flow visualization
   - See incoming requests (last 50)
   - Inspect harmonized output
   - Click "View Table Info" for database details

### Example: IoT Sensor Integration

**Workflow**: IoT Sensor Data Processor  
**Team**: Acme / Production / IoT Platform  
**Entity**: sensor_readings  
**Table**: `acme_production_iot_sensor_readings`

**Input** (various sensor formats):
```json
{
  "deviceId": "TEMP-001",
  "temp_celsius": 23.5,
  "humidity_pct": 65,
  "ts": "2026-01-20T10:30:00Z"
}
```

**Output** (unified format):
```json
{
  "device": {
    "id": "TEMP-001",
    "type": "temperature_sensor"
  },
  "timestamp": "2026-01-20T10:30:00Z",
  "sensors": [
    {"name": "temperature", "value": 23.5, "unit": "celsius"},
    {"name": "humidity", "value": 65, "unit": "percent"}
  ]
}
```

## Project Structure

```
dataharmonizer/
├── Harmonizer/                    # .NET 8 Backend
│   ├── Controllers/               # API endpoints
│   ├── Models/                    # Entities (Workflow, Request, Team, etc.)
│   ├── Services/
│   │   ├── LLM/                  # LLM integration
│   │   ├── DynamicTableService   # Auto table creation
│   │   └── ApiForwardingService  # External API routing
│   ├── Repositories/              # Data access layer
│   ├── DTOs/                      # Data transfer objects
│   └── Data/                      # EF Core context & seeder
│
├── Frontend/                      # React + TypeScript
│   └── src/
│       ├── features/
│       │   ├── workflows/         # Workflow management
│       │   ├── ingest/           # Manual data entry
│       │   ├── requests/         # Request history
│       │   └── settings/         # Organization mgmt
│       ├── api/                  # API client
│       └── types/                # TypeScript definitions
│
└── Documentation/
    ├── PROJECT_OVERVIEW.md        # Complete architecture
    └── TEAM_SELECTION_FEATURE.md  # Multi-tenancy guide
```

## API Documentation

### Workflows
```
GET    /api/Workflows                    # List all workflows
POST   /api/Workflows                    # Create workflow
PUT    /api/Workflows/{id}               # Update workflow
DELETE /api/Workflows/{id}               # Delete workflow
POST   /api/Workflows/{id}/regenerate-key # New API key
POST   /api/Workflows/webhook/{apiKey}   # Data ingestion
```

### Multi-Tenancy
```
GET    /api/Tenants/hierarchy    # Full org tree
POST   /api/Tenants             # Create tenant
POST   /api/Namespaces          # Create namespace
POST   /api/Teams               # Create team
```

### Processing
```
GET    /api/HarmonizeRequests/by-workflow/{id}  # Requests by workflow
POST   /api/HarmonizedRecords/harmonize/{id}    # Trigger harmonization
```

**Interactive Docs**: http://localhost:5144/swagger

## Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=harmonizer.db"
  },
  "LLM": {
    "BaseUrl": "http://localhost:1234/v1",
    "Model": "local-model",
    "MaxTokens": 2000
  }
}
```

### Frontend (`vite.config.js`)

```javascript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5144',
        changeOrigin: true
      }
    }
  }
})
```

## Use Cases

### 1. IoT Platform
Harmonize sensor data from multiple vendors into unified analytics format.

### 2. Manufacturing
Standardize equipment telemetry (CNC, robots, 3D printers) for OEE tracking.

### 3. Quality Control
Transform inspection results from various systems into unified reports.

### 4. System Integration
Harmonize and forward data to external platforms (Databricks, Snowflake).

## Production Deployment

### Database Migration
Replace SQLite with PostgreSQL:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=prod-db;Database=harmonizer;User Id=...;Password=..."
}
```

### Frontend Build
```bash
cd Frontend
npm run build
# Deploy dist/ to Azure Static Web Apps, Netlify, or NGINX
```

### Backend Deployment
```bash
cd Harmonizer
dotnet publish -c Release -o ./publish
# Deploy to Azure App Service, AWS Elastic Beanstalk, or Docker
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "Harmonizer.dll"]
```

## Features Roadmap

**Current (v2.0)**
- ✅ Multi-tenant architecture
- ✅ Workflow management with team assignment
- ✅ Dynamic table creation
- ✅ Request pipeline visualization
- ✅ Table schema viewer
- ✅ LLM integration (LM Studio)

**Phase 2**
- [ ] API polling configuration UI
- [ ] File upload (CSV, JSON, XML)
- [ ] Scheduled workflows
- [ ] Batch processing

**Phase 3 - Enterprise**
- [ ] User authentication & RBAC
- [ ] Audit logging
- [ ] Data retention policies
- [ ] Advanced monitoring
- [ ] Workflow versioning

**Phase 4 - Intelligence**
- [ ] Auto-prompt generation from samples
- [ ] Schema suggestions
- [ ] Data quality scoring
- [ ] Anomaly detection

## Tech Stack

**Backend**
- .NET 8, ASP.NET Core Web API
- Entity Framework Core 8
- SQLite (dev), PostgreSQL/SQL Server (prod)

**Frontend**
- React 18 with TypeScript
- Vite (build tool)
- No CSS framework (clean inline styles)

**LLM**
- LM Studio (local inference)
- Extensible to OpenAI, Anthropic Claude, Azure OpenAI

## Documentation

- **[PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md)**: Complete architecture and design philosophy
- **[TEAM_SELECTION_FEATURE.md](TEAM_SELECTION_FEATURE.md)**: Multi-tenancy configuration
- **API Docs**: http://localhost:5144/swagger (interactive)

## Contributing

This is a demonstration project showcasing modern data harmonization architecture. For production use, consider:

- Adding authentication (JWT/OAuth)
- Implementing rate limiting
- Setting up monitoring (Application Insights)
- Configuring CI/CD pipelines
- Adding comprehensive tests

## License

MIT License

## Support

For questions or issues, please open an issue in the repository.

---

**Built for**: Enterprise data integration, automated semantic mapping, IoT platforms, manufacturing systems, and scalable data harmonization.

**Version**: 2.0  
**Last Updated**: January 2026
