# PGMQ Admin UI – Blazor Server-Side Rendered App Scaffold

## Executive Summary

This document outlines a high-level implementation plan for a **Blazor Server-Side Rendered (SSR) admin UI** for managing PostgreSQL Message Queue (PGMQ) instances. Built with **.NET 10**, it will provide queue management, message inspection, and basic operations via a single-page admin dashboard running as a sidecar container.

**Target Stack:**

- **Backend:** ASP.NET Core 10 (Blazor Server SSR)
- **Database Access:** Npgmq 1.5.0+ (PGMQ .NET client)
- **UI Components:** Microsoft Fluent UI Blazor 4.13.1+ (free, open-source)
- **Deployment:** Docker container with configurable Postgres connectivity (local or remote)

---

## Architecture Overview

### High-Level Components

┌─────────────────────────────────────┐
│ User Browser │
│ (Admin Dashboard) │
└────────┬────────────────────────────┘
│ HTTP / WebSocket (Blazor)
│
┌────────▼────────────────────────────┐
│ Blazor Server App (.NET 10) │
│ ├─ Razor Pages/Components │
│ ├─ Backend Services (C#) │
│ ├─ PGMQ API Layer │
│ └─ Dependency Injection │
└────────┬────────────────────────────┘
│ Npgmq (async PGMQ operations)
│
┌────────▼────────────────────────────┐
│ PostgreSQL Instance (PGMQ Ext.) │
│ ├─ pgmq.meta (queue metadata) │
│ ├─ pgmq.q*\* (queue tables) │
│ ├─ pgmq.a*\* (archive tables) │
│ └─ PGMQ Functions │
└─────────────────────────────────────┘

### Deployment Model

- **Single Blazor Server container** runs the admin UI (port 8080).
- **Configurable Postgres connectivity** (local Docker network OR remote instance):
  - Local (Docker Compose): `Host=postgres;Port=5432`
  - Remote: `Host=your.postgres.host;Port=5432;Username=user;Password=pass`
- **Stateless backend** – all state in Postgres (can scale horizontally).
- **Health check endpoint** (e.g., `/health`) verifies Postgres/PGMQ reachability.
- **Environment config** via Docker env vars (connection string, optional API key for admin access).

---

## Tech Stack & Dependencies

### Core Framework

| Component         | Version  | Purpose            | Docs                                                                                                                          |
| ----------------- | -------- | ------------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| **.NET**          | 10.0 LTS | Runtime & SDK      | [.NET 10 Announcement](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)                                     |
| **ASP.NET Core**  | 10.0     | Web framework      | [ASP.NET Core 10 Docs](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-10.0)                                   |
| **Blazor Server** | 10.0     | UI rendering (SSR) | [Blazor Server Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-10.0#blazor-server) |

### Database & PGMQ Client

| Package    | Version | Purpose                                                              | Docs                                                  |
| ---------- | ------- | -------------------------------------------------------------------- | ----------------------------------------------------- |
| **Npgmq**  | 1.5.0+  | .NET client library for PGMQ (wraps Npgsql, auto JSON serialization) | [Npgmq GitHub](https://github.com/brianpursley/Npgmq) |
| **Npgsql** | 10.0.0+ | PostgreSQL ADO.NET provider (dependency of Npgmq)                    | [Npgsql Docs](https://www.npgsql.org)                 |

### UI Components

| Package                        | Version | Tier     | Pros                                                                | Cons                                  | Docs                                                |
| ------------------------------ | ------- | -------- | ------------------------------------------------------------------- | ------------------------------------- | --------------------------------------------------- |
| **Microsoft Fluent UI Blazor** | 4.13.1+ | Free/OSS | Modern design, free, Fluent Design System, DataGrid, buttons, forms | Still evolving (v5 coming early 2025) | [Fluent UI Blazor](https://www.fluentui-blazor.net) |

**Selected:** **Microsoft Fluent UI Blazor** 4.13.1+ (free, OSS, actively maintained by Microsoft).

**Why Fluent UI?**

- ✅ Free and open-source (zero license costs)
- ✅ Native Blazor Web Components (performant, no JavaScript overhead)
- ✅ FluentDataGrid with built-in sorting, filtering, pagination
- ✅ Complete component library: FluentButton, FluentTextField, FluentTabs, FluentDialog, FluentMessageBox
- ✅ Consistent with modern Microsoft Fluent Design System
- ✅ Active maintenance and Microsoft backing

### Additional Libraries (Sample .csproj)

<ItemGroup>
  <PackageReference Include="Npgmq" Version="1.5.0" />
  <PackageReference Include="Npgsql.DependencyInjection" Version="10.0.0" />
  <PackageReference Include="Microsoft.FluentUI.AspNetCore.Components" Version="4.13.1" />
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
</ItemGroup>

**Installation command:**
dotnet add package Npgmq
dotnet add package Npgsql.DependencyInjection
dotnet add package Microsoft.FluentUI.AspNetCore.Components

---

## Feature Breakdown (MVP)

### Phase 1: Core Dashboard

**Views & Components (using Fluent UI Blazor):**

1. **Queues Overview Page**

   - List all queues from connected Postgres instance (via Npgmq)
   - Display: queue name, total message count, in-flight count, archived count
   - Action buttons: create queue, delete queue, view detail
   - Sort/filter by name or message count (FluentDataGrid built-in features)
   - **Tech:** Razor page + FluentDataGrid (with sorting/filtering)

2. **Queue Detail Page**

   - Tabs: "Messages", "Archived", "Metrics" using FluentTabs
   - **Messages Tab:**
     - Paginated FluentDataGrid of queue messages fetched via Npgmq.ReadAsync()
     - Columns: msg_id, message (JSON, truncated with pretty-print toggle), vt (visibility), created_at, read_ct
     - Actions per row: delete, archive, requeue, extend visibility timeout
   - **Archived Tab:**
     - Similar grid view for archived messages
   - **Metrics Tab:**
     - Simple KPI cards: total sent, total received, active messages, oldest unread age
   - **Tech:** Razor component + FluentDataGrid + FluentTabs + JSON prettify utility

3. **Send Message Form**
   - Input: queue name, message (JSON text area with syntax highlighting), optional delay in seconds
   - Validation: queue must exist, message must be valid JSON
   - Button: "Send Message" (calls Npgmq.SendAsync())
   - Success/error notification via FluentMessageBox
   - **Tech:** Razor component + EditForm + JSON validator + Npgmq client

### Phase 2: Operations & Settings

4. **Queue Operations**

   - Create Queue dialog: name, VT, delay, max size
   - Delete Queue confirmation
   - Purge Queue (clear all messages)
   - Set Queue Attributes (visibility, delay)
   - **Tech:** FluentDialog, FluentButton, FluentTextField form validation

5. **Search & Filters**

   - Full-text search over queue names
   - Filter by queue type (regular, partitioned, DLQ)
   - JSON message body search (via `message->>'field'` in SQL)

6. **Health & Diagnostics**
   - Simple status indicator: Postgres online/offline
   - PGMQ extension version check
   - Queue health (e.g., oldest unread message age)

---

## Backend Service Layer (C# Structure)

### Folder Layout

PgmqBlazorUI/
├── Program.cs # DI, middleware setup
├── appsettings.json # Config (DB conn string)
├── Pages/
│ ├── Index.razor # Dashboard overview
│ ├── QueueDetail.razor # Single queue view
│ └── Layout.razor # Shared layout
├── Components/
│ ├── QueueGrid.razor # Reusable grid component
│ ├── MessageForm.razor # Send message form
│ └── Notifications.razor # Toast/alerts
├── Services/
│ ├── IPgmqService.cs # Interface
│ ├── PgmqService.cs # Implementation (Npgsql calls)
│ └── PgmqModels.cs # DTOs (Queue, Message, etc.)
├── Data/
│ ├── PgmqContext.cs # Optional EF Core DbContext
│ └── Migrations/
└── Dockerfile # Docker image definition

### Sample Service Interface (PgmqService.cs)

public interface IPgmqService
{
Task<IEnumerable<QueueDto>> ListQueuesAsync(CancellationToken ct = default);
Task<QueueDetailDto> GetQueueDetailAsync(string queueName, int page, int pageSize, CancellationToken ct = default);
Task<string> SendMessageAsync(string queueName, string jsonMessage, int? delaySeconds = null, CancellationToken ct = default);
Task<bool> DeleteMessageAsync(string queueName, string msgId, CancellationToken ct = default);
Task<bool> ArchiveMessageAsync(string queueName, string msgId, CancellationToken ct = default);
Task<bool> CreateQueueAsync(string queueName, int vt = 30, int delay = 0, CancellationToken ct = default);
Task<bool> DeleteQueueAsync(string queueName, CancellationToken ct = default);
}

**Implementation:** Uses Npgmq client library to:

- Wrap `NpgmqClient` for queue operations (send, read, archive, create, delete)
- Leverage built-in async methods from Npgmq
- Simplifies PGMQ function calls compared to raw Npgsql
- Includes auto-serialization/deserialization of JSON messages

**Example Npgmq usage in service:**
public class PgmqService : IPgmqService
{
private readonly NpgmqClient \_pgmq;
private readonly ILogger<PgmqService> \_logger;

    public PgmqService(string connectionString, ILogger<PgmqService> logger)
    {
        _pgmq = new NpgmqClient(connectionString);
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string queueName, string jsonMessage, int? delaySeconds = null, CancellationToken ct = default)
    {
        try
        {
            var msgId = await _pgmq.SendAsync(queueName, jsonMessage, delaySeconds);
            _logger.LogInformation("Message {MsgId} sent to {QueueName}", msgId, queueName);
            return msgId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to {QueueName}", queueName);
            throw;
        }
    }

    public async Task<bool> ArchiveMessageAsync(string queueName, string msgId, CancellationToken ct = default)
    {
        try
        {
            await _pgmq.ArchiveAsync(queueName, msgId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive message {MsgId}", msgId);
            return false;
        }
    }

}

### Key Considerations

- **Connection Pooling:** Npgsql + .NET 10 handle this automatically
- **Async/Await:** Use async methods throughout for Blazor Server
- **Logging:** Use `ILogger<T>` for diagnostics
- **Error Handling:** Wrap Postgres/PGMQ errors in application-level exceptions

---

## Deployment & Configuration

### Docker Setup

**Dockerfile** (multi-stage build):

# Build stage

FROM mcr.microsoft.com/dotnet/sdk:10 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage

FROM mcr.microsoft.com/dotnet/aspnet:10
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings\_\_Pgmq="Host=postgres;Port=5432;Database=postgres;Username=pgmq_admin;Password=secure_password"
ENTRYPOINT ["dotnet", "PgmqBlazorUI.dll"]

**Docker Compose** (sample):
services:
postgres:
image: postgres:16-alpine
environment:
POSTGRES_DB: postgres
POSTGRES_USER: pgmq_admin
POSTGRES_PASSWORD: secure_password
ports: - "5432:5432"
volumes: - postgres_data:/var/lib/postgresql/data

pgmq-ui:
build: .
ports: - "8080:8080"
depends_on: - postgres
environment:
ConnectionStrings\_\_Pgmq: "Host=postgres;Port=5432;Database=postgres;Username=pgmq_admin;Password=secure_password"
restart: unless-stopped

volumes:
postgres_data:

### Environment Configuration

- **Connection String:** Via `appsettings.json` or env var `ConnectionStrings__Pgmq`
- **Auth:** Simple API key or JWT (optional, can sit behind reverse proxy)
- **CORS:** Configure for reverse proxy (e.g., Traefik, Caddy)
- **Health Check:** GET `/health` returns 200 if Postgres/PGMQ is reachable

---

## Development Roadmap

**Current Status:** Steps 1 and 2 complete. Backend service layer fully implemented with comprehensive test coverage. Project is buildable and ready for UI development.

**Progress Summary:**
- ✅ **Step 1:** Project Setup - COMPLETE
- ✅ **Step 2:** Backend Service Layer - COMPLETE (services, DTOs, metrics, unit tests, integration tests)
- ⬜ **Step 3:** UI Components - NOT STARTED
- 🟨 **Step 4:** Polish & Deployment - PARTIALLY COMPLETE (infrastructure done, need testing & polish)

---

### Step 1: Project Setup ✅ **COMPLETED**

- [x] Create new `dotnet new blazor` project (.NET 10)
- [x] Install NuGet packages: `Npgmq`, `Microsoft.FluentUI.AspNetCore.Components`
- [x] Set up `appsettings.json` with Postgres connection string (support both local Docker and remote)
- [x] Add connection string to Program.cs DI via `AddNpgsqlConnection()` or manual string configuration
- [x] Create service wrappers around Npgmq client
- [x] Add Dockerfile for production deployment
- [x] Create .NET Aspire AppHost for local development orchestration
- **Docs:** [Create Blazor app](https://learn.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-10.0)

**Implementation Notes:**
- ✅ Full Blazor Server SSR project scaffolded with .NET 10
- ✅ Npgmq 1.6.0, Microsoft.FluentUI.AspNetCore.Components 4.13.1 installed
- ✅ Central package management via Directory.Packages.props
- ✅ **Vertical Slice Architecture** implemented (Features/Queues, Features/Messages) instead of traditional Services/Models layering
- ✅ QueueService and MessageService created with NpgmqClient integration
- ✅ DTOs created: QueueDto, QueueDetailDto, MessageDto
- ✅ Program.cs configured with DI, Fluent UI, and Aspire service defaults
- ✅ AppHost.cs configured with PostgreSQL + PGMQ extension (Tembo pg18-pgmq v1.7.0) and PgAdmin
- ✅ Multi-stage Dockerfile with health checks
- ✅ Build succeeds: 0 warnings, 0 errors
- 🎯 **Architecture Decision:** No IPgmqService interface (follows AGENTS.md: "No interfaces for single implementations")

### Step 2: Backend Service Layer (Day 2–3) - ✅ **COMPLETED**

- [x] Create `QueueService` class wrapping NpgmqClient instance from Npgmq package
- [x] Create `MessageService` class wrapping NpgmqClient instance from Npgmq package
- [x] Implement core methods: ListQueuesAsync(), SendMessageAsync(), ArchiveMessageAsync(), DeleteMessageAsync()
- [x] Write DTOs: QueueDto, MessageDto, QueueDetailDto
- [x] Add structured logging with ILogger for all operations (using LoggerMessage source generators)
- [x] Implement error handling: try-catch with logging in all service methods
- [x] Implement GetQueueStatsAsync() for metrics (total/in-flight/archived counts, oldest unread age)
- [x] Write QueueStatsDto
- [x] Unit test QueueService with FakeItEasy mocks
- [x] Unit test MessageService with FakeItEasy mocks
- [x] Integration tests against test Postgres instance using Testcontainers
- **Docs:** [Npgmq GitHub](https://github.com/brianpursley/Npgmq), [Npgmq Usage Examples](https://github.com/brianpursley/Npgmq#usage)

**Implementation Notes:**
- ✅ Services use vertical slice architecture (Features/Queues, Features/Messages)
- ✅ Concrete classes without interfaces (follows AGENTS.md guidance)
- ✅ LoggerMessage source generators for high-performance logging
- ✅ Virtual methods for test mocking without interfaces
- ✅ GetQueueStatsAsync() implemented using pgmq.metrics() SQL function
- ✅ QueueStatsDto with all required fields (QueueName, QueueLength, NewestMsgAgeSec, OldestMsgAgeSec, TotalMessages, ScrapeTime)
- ✅ Comprehensive unit test coverage: 15 tests for QueueService, 13 tests for MessageService (26 tests total, all passing)
- ✅ Integration test coverage: 15 integration tests using Testcontainers with PostgreSQL + PGMQ extension
- ✅ Tests follow TUnit conventions with Category attributes for filtering

### Step 3: UI Components (Day 4–5)

- [ ] Build "Queues Overview" Razor page with FluentDataGrid (fetch via PgmqService.ListQueuesAsync())
- [ ] Build "Queue Detail" Razor page with FluentTabs (Messages/Archived/Metrics tabs)
- [ ] Build "Send Message" modal dialog with FluentTextField (queue, JSON body), FluentButton (Send)
- [ ] Add global navigation header with status indicator (Postgres connection: online/offline)
- [ ] Implement FluentMessageBox for notifications (success, error, warning)
- [ ] Add JSON pretty-print toggle on message detail view
- **Docs:** [Fluent UI Blazor](https://www.fluentui-blazor.net), [DataGrid API](https://learn.microsoft.com/en-us/fluent-ui/web-components/components/data-grid)

### Step 4: Polish & Deployment (Day 6) - **PARTIALLY COMPLETE**

- [x] Add `/health` endpoint: returns 200 if Postgres/PGMQ is reachable (via Aspire MapDefaultEndpoints)
- [x] Configure Docker build (multi-stage) with health check
- [ ] Configure Docker Compose with optional Postgres service
- [ ] Test with both local Docker Postgres and remote instance (via env var override)
- [ ] Test UI against live queue operations (send/read/archive cycle)
- [ ] Add error messages for common failures (connection failed, queue not found, invalid JSON)
- [ ] Polish styling: Fluent UI dark/light mode support, responsive layout
- **Docs:** [Blazor Performance](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/?view=aspnetcore-10.0)

**Implementation Notes:**
- ✅ Health checks configured via Aspire service defaults (MapDefaultEndpoints)
- ✅ Dockerfile includes HEALTHCHECK directive
- ⏳ **Remaining:** Docker Compose, end-to-end testing, error handling, UI polish

---

## Key Links & Resources

### Official Documentation

- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core 10 Docs](https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-10.0)
- [Blazor Server Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-10.0#blazor-server)
- [Blazor Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/?view=aspnetcore-10.0)

### Database & ORM

- [Npgsql Official Docs](https://www.npgsql.org)
- [Npgsql + Entity Framework Core](https://www.npgsql.org/doc/usage/entity-framework-core.html)
- [PGMQ GitHub Repository](https://github.com/pgmq/pgmq)
- [PGMQ Supabase Docs](https://supabase.com/docs/guides/database/extensions/pgmq)

### UI Components & Styling

- [Microsoft Fluent UI Blazor](https://www.fluentui-blazor.net)
- [Fluent UI DataGrid Component](https://learn.microsoft.com/en-us/fluent-ui/web-components/components/data-grid)
- [Fluent UI Blazor Components & Examples](https://github.com/microsoft/fluentui-blazor)

### Blazor Learning & Patterns

- [Build Your First Blazor App](https://learn.microsoft.com/en-us/aspnet/core/blazor/tutorials/build-a-blazor-app?view=aspnetcore-10.0)
- [Blazor Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-10.0)
- [Blazor Form Validation](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-10.0)

### Deployment & DevOps

- [Docker & ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [Docker Compose for .NET](https://learn.microsoft.com/en-us/dotnet/core/docker/docker-compose)

### Blog Posts & Tutorials

- [Blazor & .NET 10 Developer's Guide](https://www.gapvelocity.ai/blog/blazor-dotnet-10-a-developers-guide)
- [.NET 10 – Everything You NEED to KNOW](https://www.youtube.com/watch?v=AbD4XST2Pcw&vl=en)
- [ASP.NET Core & Blazor in .NET 10](https://www.youtube.com/watch?v=xZ26KwGHWE0)

---

## Notes for AI Agent

### Next Steps (Agent Handoff)

1. **Scaffold the project structure** using the folder layout above
2. **Implement PgmqService** with Npgsql calls for core operations (list queues, send/read/delete messages)
3. **Build Razor pages** (Index, QueueDetail) with Blazor Bootstrap Grid for message display
4. **Add form validation** and error handling throughout
5. **Dockerize** the application and test in sidecar mode
6. **Polish UX** with animations, dark mode, and accessibility features

### Assumptions

- ✅ PGMQ extension is already installed on Postgres instance (local or remote)
- ✅ User has `.NET 10 SDK` installed locally for development
- ✅ Postgres connection string is provided via environment variable or appsettings.json
- ✅ Reverse proxy (Traefik, Caddy, nginx) or simple auth middleware handles API security (optional)

### Success Criteria

- ✅ UI displays queue list and messages fetched from live Postgres/PGMQ instance
- ✅ Can create, delete, send, archive messages via the dashboard
- ✅ Runs in Docker and connects to Postgres (both local Docker and remote instances)
- ✅ `/health` endpoint returns 200 when Postgres/PGMQ is reachable
- ✅ All config (connection string, API key) via environment variables; zero hardcoded secrets
- ✅ Connection string supports Docker internal DNS (`postgres:5432`) AND remote hosts
- ✅ Error messages displayed for connection failures, invalid operations
