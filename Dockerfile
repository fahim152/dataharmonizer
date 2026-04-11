# ============================================================
# Stage 1: Build the React frontend
# ============================================================
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Install dependencies first (layer caching)
COPY Frontend/package.json Frontend/package-lock.json* ./
RUN npm install

# Copy frontend source and build
COPY Frontend/ ./

# When VITE_API_BASE_URL is empty, the frontend uses relative URLs (same origin)
# which is what we want when the backend serves the SPA.
ENV VITE_API_BASE_URL=""
RUN npm run build

# ============================================================
# Stage 2: Build the .NET backend
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /src

# Restore dependencies first (layer caching)
COPY Harmonizer/Harmonizer.csproj Harmonizer/
RUN dotnet restore Harmonizer/Harmonizer.csproj

# Copy the rest of the backend source and publish
COPY Harmonizer/ Harmonizer/
RUN dotnet publish Harmonizer/Harmonizer.csproj -c Release -o /app/publish --no-restore

# ============================================================
# Stage 3: Final runtime image
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published backend
COPY --from=backend-build /app/publish .

# Copy built frontend into wwwroot so the backend can serve it
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Expose port 8080 (ASP.NET default in containers)
EXPOSE 8080

# Environment variables — override at runtime as needed
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# SQLite DB will be stored in /app/data (mount a volume for persistence)
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/harmonizer.db"

# LLM settings — override these at runtime via env vars or docker-compose
ENV LLM__BaseUrl="http://host.docker.internal:1234"
ENV LLM__Model="openai/gpt-oss-20b"
ENV LLM__Temperature="0.2"
ENV LLM__MaxTokens="4096"

# Create data directory for SQLite
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "Harmonizer.dll"]
