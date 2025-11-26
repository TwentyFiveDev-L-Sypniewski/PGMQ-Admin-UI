# PGMQ Admin UI - Development Guide

**Stack**: .NET 10, Blazor Server SSR, .NET Aspire 13, PostgreSQL, PGMQ, Fluent UI Blazor
**Principles**: SOLID, KISS, YAGNI. Consistency over innovation.

## Project Overview

Blazor Server-Side Rendered (SSR) admin dashboard for managing PostgreSQL Message Queue (PGMQ) instances. Built with .NET 10, providing queue management, message inspection, and PGMQ operations via an intuitive web interface.

**Deployment Model**: Hybrid Aspire approach - Aspire orchestration for local development, standalone Docker container for production.

## Repository Structure

- `PgmqAdminUI/`: Main Blazor Server app
  - `Components/Pages/`: Routable pages (Index, QueueDetail)
  - `Components/Layout/`: Shared layouts
  - `Components/UI/`: Reusable UI components (QueueGrid, MessageForm)
  - `Services/`: Business logic (PgmqService)
  - `Models/`: DTOs (QueueDto, MessageDto)
- `PgmqAdminUI.AppHost/`: .NET Aspire orchestration (local dev only)
- `PgmqAdminUI.ServiceDefaults/`: Shared Aspire defaults
- `PgmqAdminUI.Tests/`: Unified test project
  - `Unit/`: Unit tests (TUnit + FakeItEasy)
  - `Integration/`: Integration tests (TUnit + Testcontainers)
  - `Component/`: Blazor component tests (bUnit + TUnit)
  - `EndToEnd/`: E2E tests (Playwright)

## Tech Stack

**Core Framework:**
- .NET 10 LTS, ASP.NET Core 10, Blazor Server SSR

**PGMQ Client:**
- Npgmq 1.5.0+ (async PGMQ operations with auto JSON serialization)

**UI Components:**
- Microsoft Fluent UI Blazor 4.13.1+ (FluentDataGrid, FluentButton, FluentDialog, FluentTabs)

**Orchestration:**
- .NET Aspire 13.0 (local development only)

**Database:**
- PostgreSQL with PGMQ extension (supports local Docker and remote instances)

**Testing:**
- TUnit, FakeItEasy, Testcontainers, bUnit, Playwright

## Key Commands

```bash
# Core
dotnet build                                    # Build solution
dotnet test                                     # Run all tests (requires Docker)
dotnet format                                   # Apply code style

# Aspire (local development)
dotnet run --project PgmqAdminUI.AppHost        # Start Aspire dashboard + app

# Docker (production deployment)
docker build -t pgmq-admin-ui ./PgmqAdminUI     # Build standalone container
docker run -p 8080:8080 \
  -e ConnectionStrings__Pgmq="Host=your.postgres.host;Port=5432;..." \
  pgmq-admin-ui                                 # Run with remote Postgres
```

## Quality Gates ✅

**Execute before finishing task:**

1. `dotnet build` (MUST PASS)
2. `dotnet test` (MUST PASS - all 4 test layers)
3. `dotnet format` (Apply style)
4. **Docs**: Update `AGENTS.md` if patterns changed.

## Coding Standards & Patterns

### Core Principles

- **SOLID, KISS, YAGNI**. Simplicity first: inline code over new classes, direct calls over abstractions.
- **No premature abstraction**: No provider/manager/factory until 3+ use cases.
- **Self-documenting code**: Descriptive names over comments. Explain **why**, not **what**.
- **Async/await** for all I/O. Proper error handling with user feedback.

### Blazor Patterns

- **Component Organization**:
  - `Pages/`: Routable components with `@page` directive
  - `Layout/`: Shared layouts (MainLayout.razor)
  - `UI/`: Reusable components (QueueGrid, MessageForm)
- **Render Modes**: Static SSR by default. Use interactive Server mode only when needed (forms, real-time updates).
- **Service Injection**: `@inject PgmqService PgmqService` in components.
- **Form Handling**: Use `EditForm` with validation, `FluentButton` for submit.

### Architecture & Examples

- **Service Layer**: `PgmqService` wraps `NpgmqClient` (from Npgmq package). No repository pattern - keep it simple.
  ```csharp
  public class PgmqService
  {
      private readonly NpgmqClient _pgmq;
      public PgmqService(string connectionString) => _pgmq = new NpgmqClient(connectionString);
      public async Task<string> SendMessageAsync(string queue, string json, int? delay = null)
          => await _pgmq.SendAsync(queue, json, delay);
  }
  ```
- **Fluent UI Components**: Use FluentDataGrid for tables, FluentDialog for modals, FluentMessageBox for notifications.
- **Aspire**: AppHost orchestrates local PostgreSQL. Production uses standalone Docker.
- **Connection Strings**: Via environment variables. Supports both:
  - Local Docker: `Host=postgres;Port=5432;Database=postgres`
  - Remote: `Host=your.postgres.host;Port=5432;Username=user;Password=pass`

### .NET 10 & C# 14 Features

- Required members for DTOs, File-scoped namespaces, Collection expressions `[]`.
- `DateOnly`/`TimeOnly`, `DateTimeOffset` > `DateTime`.
- **DI**: Private readonly fields > primary constructors
- Built-in Async LINQ and HTTP JSON extensions.

### Code Style

- **No interfaces for single implementations**: Concrete types suffice. Mocking is a last resort.
- **No XML docs**: Code must be self-documenting. Minimal section comments.
- **Commentary**: Only when intent is unclear—explain **why**, not **what**. Avoid obvious comments that restate code.
- **Logging**: `ILogger<T>` consistently.
- **Dependencies**: Update via `dotnet tool run dotnet-outdated -u:auto -r`. Manage in `Directory.Packages.props`.

## Testing

**Testing Pyramid (4 Layers):**

### Layer 1: Unit Tests (TUnit + FakeItEasy)
- **What**: PgmqService methods, business logic, DTOs
- **How**: Mock NpgmqClient interface with FakeItEasy
- **Speed**: Milliseconds
- **Location**: `PgmqAdminUI.Tests/Unit/Services/PgmqServiceTests.cs`
- **Example**:
  ```csharp
  [Test]
  public async Task SendMessageAsync_ValidQueue_ReturnsMessageId()
  {
      var fakeClient = A.Fake<NpgmqClient>();
      A.CallTo(() => fakeClient.SendAsync("test-queue", A<string>._, null))
          .Returns("msg_123");
      // Assert...
  }
  ```

### Layer 2: Integration Tests (TUnit + Testcontainers + WebApplicationFactory)
- **What**: Real PostgreSQL + PGMQ operations, HTTP endpoints
- **How**: Testcontainers.PostgreSql spins up PostgreSQL with PGMQ extension
- **Speed**: Seconds
- **Location**: `PgmqAdminUI.Tests/Integration/Services/PgmqServiceIntegrationTests.cs`
- **Example**:
  ```csharp
  [Test]
  public async Task SendMessage_ToRealQueue_CanBeRead()
  {
      // Testcontainers PostgreSQL with PGMQ
      await using var container = new PostgreSqlBuilder()
          .WithImage("quay.io/tembo/pg18-pgmq:latest")
          .Build();
      await container.StartAsync();
      // Test against real PGMQ...
  }
  ```

### Layer 3: Component Tests (bUnit + TUnit)
- **What**: Blazor components (QueueGrid, MessageForm, Pages)
- **How**: bUnit renders components, simulates button clicks, form submissions
- **Speed**: Milliseconds
- **Location**: `PgmqAdminUI.Tests/Component/UI/QueueGridTests.cs`
- **Note**: bUnit works with both static and interactive SSR components
- **Example**:
  ```csharp
  [Test]
  public void QueueGrid_RendersQueueList_DisplaysQueueNames()
  {
      using var ctx = new TestContext();
      var queues = new[] { new QueueDto { Name = "test-queue" } };
      var component = ctx.RenderComponent<QueueGrid>(parameters => parameters
          .Add(p => p.Queues, queues));
      component.Find("td").TextContent.ShouldBe("test-queue");
  }
  ```

### Layer 4: E2E Tests (Playwright)
- **What**: Full user workflows through browser
- **How**: Playwright automates browser, tests against running app
- **Speed**: Seconds
- **Location**: `PgmqAdminUI.Tests/EndToEnd/QueueManagementWorkflowTests.cs`
- **Example**:
  ```csharp
  [Test]
  public async Task UserCanCreateQueueAndSendMessage()
  {
      await using var playwright = await Playwright.CreateAsync();
      await using var browser = await playwright.Chromium.LaunchAsync();
      var page = await browser.NewPageAsync();
      await page.GotoAsync("http://localhost:8080");
      await page.ClickAsync("text=Create Queue");
      // ... complete workflow
  }
  ```

**Test Execution:**
```bash
dotnet test                                      # Run all tests
dotnet test --filter "Category=Unit"            # Unit tests only
dotnet test --filter "Category=Integration"     # Integration tests only
dotnet test --filter "Category=Component"       # Component tests only
dotnet test --filter "Category=E2E"             # E2E tests only
```

## Documentation

**MUST CHECK after every change:**

- Review `AGENTS.md` for accuracy.
- Update patterns, tools, commands, or workflows if changed.
- Ensure future developers/agents have accurate guidance.

## Agent Workflow

### Development Workflow

1. **Aspire (Local Development)**:
   ```bash
   dotnet run --project PgmqAdminUI.AppHost
   ```
   - Starts Aspire dashboard at https://localhost:17287
   - Launches PostgreSQL container with PGMQ extension
   - Runs PgmqAdminUI app

2. **PGMQ Operations**:
   - `PgmqService` wraps `NpgmqClient` from Npgmq package
   - Methods: `SendMessageAsync()`, `ReadAsync()`, `ArchiveAsync()`, `DeleteAsync()`, `CreateQueueAsync()`
   - Connection string from `IConfiguration` or environment variables

3. **Testing**:
   - Run all 4 test layers before committing
   - Unit tests: Fast feedback on service logic
   - Integration tests: Verify PGMQ operations with real PostgreSQL
   - Component tests: Verify Blazor component rendering and interactions
   - E2E tests: Verify critical user workflows

4. **Deployment**:
   - **Standalone Docker** (no Aspire dependency):
     ```bash
     docker build -t pgmq-admin-ui ./PgmqAdminUI
     docker run -p 8080:8080 \
       -e ConnectionStrings__Pgmq="Host=your.host;Port=5432;..." \
       pgmq-admin-ui
     ```
   - Supports both local Docker Postgres and remote instances
   - Health check endpoint: `GET /health`

### Common Tasks

- **Add New Queue Operation**: Update `PgmqService`, add unit test, add integration test.
- **Add New UI Component**: Create in `Components/UI/`, add bUnit test.
- **Add New Page**: Create in `Components/Pages/`, add E2E test for critical path.
- **Update Dependencies**: `dotnet tool run dotnet-outdated -u:auto -r`

### Troubleshooting

- **Testcontainers fails**: Ensure Docker is running. Check `docker ps`.
- **PGMQ extension missing**: Use `quay.io/tembo/pg18-pgmq:latest` image.
- **Connection refused**: Verify connection string, check Postgres is running.
- **Aspire dashboard not loading**: Check port 17287, restart AppHost.

## No Temp Docs

Do not commit temporary documentation files like `IMPLEMENTATION.md`, `TASKS.md`, etc. Keep guidance in `AGENTS.md` and `IMPLEMENTATION_PLAN.md` only.
