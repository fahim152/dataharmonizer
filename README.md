# Harmonizer

A simple data harmonizer API that accepts arbitrary JSON payloads from external systems, stores the raw JSON, and exposes retrieval endpoints.

## Stack
- .NET 8 Web API (minimal host + Controllers)
- EF Core 8 + SQLite
- Layered architecture: Controller -> Service -> Repository -> DbContext

## Project Structure
```
Controllers/         // API endpoints
DTOs/                // Request/response DTOs
Models/              // Entity models
Services/            // Business logic
Repositories/        // Data access abstraction
Data/                // EF Core DbContext
Migrations/          // EF Core migrations
```

## Entity
`HarmonizedRecord` stores:
- Id (int, identity)
- RawJson (string) original payload serialized
- ReceivedAt (UTC timestamp)
- Source (optional system identifier)

## Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | /api/HarmonizedRecords | Persist a JSON payload |
| GET | /api/HarmonizedRecords/{id} | Get a single record |
| GET | /api/HarmonizedRecords?skip=0&take=100 | List records with pagination |

## Sample Request
```http
POST https://localhost:7144/api/HarmonizedRecords
Content-Type: application/json
{
  "source": "SystemA",
  "payload": {"foo": "bar", "value": 42}
}
```

## Running
```cmd
REM Restore & build
dotnet restore Harmonizer.sln
dotnet build Harmonizer.sln

REM Run (Development profile)
dotnet run --project Harmonizer/Harmonizer.csproj
```
Swagger UI will be at https://localhost:7144/swagger

## Migrations
To add a migration:
```cmd
dotnet ef migrations add <Name> --project Harmonizer/Harmonizer.csproj --startup-project Harmonizer/Harmonizer.csproj --output-dir Migrations
```
Apply migrations (or rely on automatic `db.Database.Migrate()` during startup):
```cmd
dotnet ef database update --project Harmonizer/Harmonizer.csproj --startup-project Harmonizer/Harmonizer.csproj
```

## Notes & Next Steps
- Add validation profiles per source to transform/harmonize into a canonical schema.
- Introduce indexing/search over payload (e.g., extract key fields into columns).
- Add correlation IDs & structured logging.
- Implement soft delete / retention policies.
- Consider switching to PostgreSQL for production scale.
- Add unit tests for service & repository.

## License
Internal / TBD.

