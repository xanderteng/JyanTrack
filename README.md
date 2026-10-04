# JyanTrack: Personal Mahjong Soul Enterprise Telemetry & Analytics Platform

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server%202022-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Entity Framework Core](https://img.shields.io/badge/ORM-EF%20Core%208.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![Bootstrap](https://img.shields.io/badge/Frontend-Bootstrap%204.6-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**JyanTrack** is an enterprise-grade telemetry ingestion engine and performance analytics platform designed to track, normalize, and evaluate competitive Mahjong Soul match logs on my account only. Built on **ASP.NET Core 8**, **Entity Framework Core 8.0**, **Microsoft SQL Server**, and a responsive **Bootstrap / jQuery** single-page presentation tier, JyanTrack demonstrates decoupled tiered architecture, resilient upstream data ingestion, relational idempotency, and automated database migrations.

---

## Table of Contents

- [System Architecture](#system-architecture)
- [Key Features & Enterprise Patterns](#key-features--enterprise-patterns)
- [Scoring & Performance Index Formulas](#scoring--performance-index-formulas)
- [Quickstart with Docker](#quickstart-with-docker)
- [Native Local Setup (.NET CLI)](#native-local-setup-net-cli)
- [Configuration & Environment Variables](#configuration--environment-variables)
- [API Reference](#api-reference)
- [Data Privacy & Telemetry Security](#data-privacy--telemetry-security)

---

## System Architecture

JyanTrack adheres to a strictly decoupled, n-tier enterprise architecture where each layer maintains isolated responsibilities and communicates through typed contracts:

### Architecture Layer Matrix

| Layer | Primary Components | Key Responsibilities | Technologies |
| :--- | :--- | :--- | :--- |
| **Presentation Tier** | Single-Page Dashboard (`index.html`, `app.js`) | Visualizes placement distribution, KPI metric cards, and recent match logs. Triggers asynchronous background syncs. | Bootstrap 4.6, FontAwesome 5, jQuery |
| **Controller Tier** | `MahjongStatsController`, `IngestionTestController` | Exposes RESTful endpoints, validates HTTP inputs, and formats JSON responses. | ASP.NET Core 8 Web API |
| **Business Analytics Tier** | `MahjongAnalyticsService` | Executes server-side LINQ aggregations, placement statistics, and rating index calculations. | C# / LINQ Projections, DTOs |
| **Ingestion & Sync Tier** | `AmaeKoromoService`, `MatchSyncService` | Ingests upstream match logs, handles Gzip/Deflate decompression, and coordinates fallback caching. | Typed `HttpClient`, `SocketsHttpHandler` |
| **Persistence & ORM Tier** | `AppDbContext`, Migrations | Encapsulates relational mappings, manages unit of work sessions, enforces database idempotency, and handles auto-migrations. | Entity Framework Core 8.0 |
| **Relational Storage** | `JyanTrackDb` | Persists players, room records, and match placements in a normalized 3NF relational schema. | Microsoft SQL Server 2022 / LocalDB |

### Ingestion & Processing Pipeline

1. **Telemetry Ingestion**: The client requests telemetry for a given account. `AmaeKoromoService` dispatches a pooled HTTP request to upstream telemetry endpoints with automated Gzip/Deflate decompression.
2. **Resilience & Fallback Handling**: If an upstream rate limit (HTTP 429) or network drop occurs, the pipeline transitions to the local JSON fallback cache without throwing unhandled exceptions.
3. **Idempotent Ingestion**: `MatchSyncService` checks match UUIDs against `AppDbContext.MatchRecords`. Duplicate matches are discarded at the database level, and new placements are persisted transactionally.
4. **Analytics Projections**: `MahjongAnalyticsService` computes placement distribution rates, average placement ($\bar{R}$), Las avoidance, and performance rating (PR) using zero-allocation `.AsNoTracking()` LINQ queries.
5. **Dashboard Rendering**: Aggregated statistics and recent match summaries are served as lightweight JSON payloads for asynchronous DOM rendering.

---

## Key Features & Enterprise Patterns

### 1. Inversion of Control & Resilient Ingestion
- **Typed `HttpClient`**: Uses `SocketsHttpHandler` connection pooling with automatic `GZip` and `Deflate` decompression to prevent socket exhaustion during burst queries.
- **Circuit-Style Fallback**: Gracefully handles upstream HTTP 429 (Too Many Requests) or network drops without breaking user requests.
- **Connection Resiliency**: Database queries leverage `EnableRetryOnFailure` to automatically withstand transient network interruptions.

### 2. Normalized Relational Schema (3NF) & Idempotency
- **`Players` Table**: Stores canonical player accounts (`AccountId`, `Nickname`, `DanLevel`).
- **`MatchRecords` Table**: Contains room metadata (`ExternalId` UUID, `RoomMode`, timestamps). Unique indexing on `ExternalId` strictly enforces idempotency at the database engine level.
- **`MatchPlacements` Table**: Normalized bridge entity recording seat index, final placement (1st–4th), raw score, and rating delta (`UmaDelta`).

### 3. Automated Database Migrations
- EF Core migrations automatically execute on container startup with a resilient exponential retry loop. No manual CLI migration commands are required when launching with Docker Compose.

### 4. Non-Root Container Hardening
- Production runtime images execute under the unprivileged `$APP_UID` non-root user (UID 1654), minimizing host attack surface while maintaining file system safety.

---

## Scoring & Performance Index Formulas

JyanTrack evaluates competitive players against Jade Room promotion benchmarks using established statistical formulas:

| Metric | Mathematical Formula | Competitive Benchmark |
| :--- | :--- | :--- |
| **Average Placement ($\bar{R}$)** | $$\bar{R} = \frac{\sum_{i=1}^{N} \text{Rank}_i}{N}$$ | $\bar{R} < 2.40$ indicates positive climb velocity |
| **4th-Place (Las) Avoidance Rate** | $$\text{Avoidance} = \left(1 - \frac{\text{Count}(\text{Rank} = 4)}{N}\right) \times 100\%$$ | $> 80.0\%$ required for sustainable Jade progression |
| **Performance Rating (PR)** | $$\text{PR} = 1500 + \left(\frac{30 \cdot N_1 + 10 \cdot N_2 - 10 \cdot N_3 - 30 \cdot N_4}{N}\right) \times 10$$ | Weighted index calibrated to Jade score economics |

---

## Quickstart with Docker

The fastest way to deploy JyanTrack along with its SQL Server database is via Docker Compose:

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine $\ge 24.0$ with Compose v2.

### Launch Containers

```bash
# Clone the repository
git clone https://github.com/your-username/JyanTrack.git
cd JyanTrack

# (Optional) Copy environment defaults
cp .env.example .env

# Spin up both SQL Server 2022 and JyanTrack API
docker compose up --build -d
```

### Access Services
- **Dashboard Web UI**: [http://localhost:5062/](http://localhost:5062/)
- **Swagger OpenAPI Documentation**: [http://localhost:5062/swagger](http://localhost:5062/swagger)
- **SQL Server (Host Port)**: `localhost:1433` (User: `sa`, Password: `YourStrong@Password123!`)

> [!NOTE]
> Database migrations apply automatically during container boot. The API container waits for SQL Server's healthcheck to report healthy before starting.

### Stop Containers

```bash
docker compose down
```
To also purge database volumes:
```bash
docker compose down -v
```

---

## Native Local Setup (.NET CLI)

If you prefer developing directly on Windows without Docker:

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (`sqllocaldb`) or a local SQL Server instance
- `dotnet-ef` global tool (`dotnet tool install --global dotnet-ef`)

### Steps

1. **Start SQL Server LocalDB**:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

2. **Navigate to Project & Apply Migrations**:
   ```powershell
   cd JyanTrackAPI
   dotnet ef database update
   ```

3. **Run Application**:
   ```powershell
   dotnet run
   ```

4. **Open in Browser**:
   - Web UI: [http://localhost:5062](http://localhost:5062)
   - Swagger UI: [http://localhost:5062/swagger](http://localhost:5062/swagger)

---

## Configuration & Environment Variables

Settings can be specified via `.env` for Docker Compose or standard `appsettings.json` / environment variables:

| Variable | Default Value | Description |
| :--- | :--- | :--- |
| `APP_PORT` | `5062` | Host port mapped to container HTTP port 8080 |
| `MSSQL_PORT` | `1433` | Host port mapped to SQL Server container |
| `MSSQL_SA_PASSWORD` | `YourStrong@Password123!` | System administrator password for SQL Server |
| `ASPNETCORE_ENVIRONMENT` | `Development` | ASP.NET environment (`Development` activates Swagger) |
| `ConnectionStrings__DefaultConnection` | *(Set automatically in compose)* | ADO.NET SQL Server connection string |

---

## API Reference

### 1. Player Analytics & Match History
`GET /api/MahjongStats/profile/{accountId}`

Fetches computed metrics, placement percentages, and the 10 most recent matches for an account.

**Sample Request:**
```bash
curl -X GET "http://localhost:5062/api/MahjongStats/profile/12345678" -H "accept: application/json"
```

**Sample Response (200 OK):**
```json
{
  "accountId": 12345678,
  "nickname": "ExamplePlayer",
  "danLevel": 10401,
  "totalMatches": 45,
  "firstPlaceCount": 14,
  "secondPlaceCount": 13,
  "thirdPlaceCount": 11,
  "fourthPlaceCount": 7,
  "firstPlaceRate": 31.11,
  "secondPlaceRate": 28.89,
  "thirdPlaceRate": 24.44,
  "fourthPlaceRate": 15.56,
  "averageRank": 2.24,
  "lasAvoidanceRate": 84.44,
  "averageScore": 28450.0,
  "totalUmaDelta": 340,
  "performanceRating": 1566.7,
  "recentMatches": [
    {
      "externalId": "261004-ef66045e-bea7-4faa-b472-f805dd5aabe0",
      "playedAt": "2026-10-04T19:28:03Z",
      "rank": 1,
      "finalScore": 48200,
      "umaDelta": 65
    }
  ]
}
```

### 2. Live Match Synchronization
`POST /api/MahjongStats/sync/{accountId}`

Fetches recent matches from upstream telemetry, deduplicates existing matches, records placements, and updates stats.

**Sample Request:**
```bash
curl -X POST "http://localhost:5062/api/MahjongStats/sync/12345678" -H "accept: application/json"
```

### 3. Database Summary Audit
`GET /api/IngestionTest/db/summary`

Returns row counts and sample records across players and matches in the database.

### 4. Search Player by Nickname
`GET /api/IngestionTest/player/{nickname}`

Searches upstream directory for player handles and returns matching account IDs.

---

## Data Privacy & Telemetry Security

- **No Hardcoded Accounts**: The frontend input starts clean without pre-filling any personal account ID. Optional URL parameters (`?id=12345678`) can be used for direct dashboard links.
- **Git & Docker Ignores**: All offline JSON dumps and telemetry caches (`player_records.json`, `player_extended_stats.json`, `*.json`) are strictly excluded in both `.gitignore` and `.dockerignore`.
- **Secret Separation**: Sensitive connection strings and SQL passwords are configurable via `.env` rather than baked into images or committed to source control.
