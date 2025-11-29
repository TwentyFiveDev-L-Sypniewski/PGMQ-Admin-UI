# PGMQ Admin UI - Development Guide

**Stack**: .NET 10, Blazor Server SSR, .NET Aspire 13, PostgreSQL, PGMQ, Fluent UI Blazor
**Principles**: SOLID, KISS, YAGNI. Consistency over innovation.

## Project Overview

Blazor Server-Side Rendered (SSR) admin dashboard for managing PostgreSQL Message Queue (PGMQ) instances. Built with .NET 10, providing queue management, message inspection, and PGMQ operations via an intuitive web interface.

**Deployment Model**: Hybrid Aspire approach - Aspire orchestration for local development, standalone Docker container for production.

## Repository Structure

**Architecture**: Vertical Slices - organized by feature, not by technical layer.

- `PgmqAdminUI/`: Main Blazor Server app
  - `Features/`: Feature slices (Queues, Messages) - each contains DTOs, services, and components
  - `Components/`: Shared Blazor components (Pages, Layout, UI)
- `PgmqAdminUI.AppHost/`: .NET Aspire orchestration (local dev only)
- `PgmqAdminUI.ServiceDefaults/`: Shared Aspire defaults
- `PgmqAdminUI.Tests/`: Unified test project (Unit, Integration, Component)

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
dotnet test                                     # Run all tests
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
2. `dotnet test` (MUST PASS)
3. `dotnet format` (Apply style)
4. **Docs**: Update `AGENTS.md` if patterns changed.

## Coding Standards & Patterns

### Core Principles

- **SOLID, KISS, YAGNI**. Simplicity first: inline code over new classes, direct calls over abstractions.
- **No premature abstraction**: No provider/manager/factory until 3+ use cases.
- **Self-documenting code**: Descriptive names over comments. Explain **why**, not **what**.
- **Async/await** for all I/O. Proper error handling with user feedback.

### Blazor Patterns

- **Vertical Slices**: Organize by feature, not by technical layer. Each feature contains its own DTOs, services, and components.
- **Feature Organization**:
  - `Features/{FeatureName}/`: Contains all code for a specific feature
  - DTOs, services, and feature-specific components live together
  - Shared components go in `Components/UI/`
- **Render Modes**: Static SSR by default. Use interactive Server mode only when needed (forms, real-time updates).
- **Service Injection**: `@inject QueueService QueueService` in components.
- **Form Handling**: Use `EditForm` with validation, `FluentButton` for submit.

### Architecture & Examples

- **Vertical Slices**: Each feature (`Features/{FeatureName}/`) is self-contained with its own DTOs, services, and components.
- **Service Layer**: `QueueService` and `MessageService` wrap `NpgmqClient` (from Npgmq package). No repository pattern.
- **Fluent UI Components**: Use FluentDataGrid for tables, FluentDialog for modals, FluentMessageBox for notifications.
- **Aspire**: AppHost orchestrates local PostgreSQL. Production uses standalone Docker.
- **Connection Strings**: Via environment variables. Supports both local Docker and remote instances.

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

See `PgmqAdminUI.Tests/AGENTS.md` for detailed testing guidance.

**Quick Commands:**

```bash
dotnet test                                              # Run all tests
dotnet test -- --treenode-filter "/*/*/*/*[Category=Unit]"         # Unit tests only
dotnet test -- --treenode-filter "/*/*/*/*[Category=Integration]"  # Integration tests only
dotnet test -- --treenode-filter "/*/*/*/*[Category=Component]"    # Component tests only
```

**Note:** TUnit uses `--treenode-filter` ([docs](https://tunit.dev/docs/execution/test-filters/)). The `--` separates dotnet test args from TUnit args. Pattern follows the test tree hierarchy: `/Assembly/Namespace/Class/Test` — use `*` wildcards to match any segment (e.g., `/*/*/*/*[Category=Unit]` matches all tests with Category=Unit across all assemblies, namespaces, and classes).

### Bug-Fix Testing Pattern

When fixing a bug, write tests FIRST that assert the expected (correct) behavior:

1. **Write production-grade tests**: Assert what the code SHOULD do, not "reproduce the bug"
2. **Tests fail initially**: Confirms the bug exists
3. **Implement the fix**: Tests now pass
4. **No "bug" comments**: Tests should read like normal feature tests — no "REPRODUCES THE ISSUE" or "CURRENT BUG" language

Example: If clicking a button triggers wrong action, write a test asserting the correct action. Test fails → confirms bug. Fix code → test passes.

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

   - `QueueService` and `MessageService` wrap `NpgmqClient` from Npgmq package
   - Queue methods: `ListQueuesAsync()`, `CreateQueueAsync()`, `DeleteQueueAsync()`, `GetQueueDetailAsync()`
   - Message methods: `SendMessageAsync()`, `DeleteMessageAsync()`, `ArchiveMessageAsync()`
   - Connection string from `IConfiguration` or environment variables

3. **Testing**:

   - Run all tests with `dotnet test` before committing

### Common Tasks

- **Add New Queue Operation**: Update `QueueService` in `Features/Queues/`, add unit test, add integration test.
- **Add New Message Operation**: Update `MessageService` in `Features/Messages/`, add unit test, add integration test.
- **Add New UI Component**: Create in feature folder (e.g., `Features/Queues/QueueGrid.razor`), add bUnit test. Use `Components/UI/` only for truly shared components.
- **Add New Page**: Create in feature folder or `Components/Pages/` for shared pages, add test for critical path.
- **Update Dependencies**: `dotnet tool run dotnet-outdated -u:auto -r`

## No Temp Docs

Do not commit temporary documentation files like `IMPLEMENTATION.md`, `TASKS.md`, etc. Keep guidance in `AGENTS.md` and `IMPLEMENTATION_PLAN.md` only.
