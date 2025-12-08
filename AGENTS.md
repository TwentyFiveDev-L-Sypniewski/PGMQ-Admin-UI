# PGMQ Admin UI - Development Guide

**Stack**: .NET 10, Blazor Server SSR, .NET Aspire 13, PostgreSQL, PGMQ, Fluent UI Blazor  
**Principles**: SOLID, KISS, YAGNI. Consistency over innovation.

## Project Overview

Admin dashboard for PGMQ instances. Aspire for local dev with PostgreSQL, standalone Docker for production.

## Repository Structure

**Architecture**: Vertical Slices - organized by feature, not technical layer.

- `PgmqAdminUI/`: Main app (`Features/` for slices, `Components/` for shared UI)
- `PgmqAdminUI.AppHost/`: Aspire orchestration (local dev only)
- `PgmqAdminUI.ServiceDefaults/`: Shared Aspire defaults
- `PgmqAdminUI.Tests/`: Unit, Integration, Component tests

## Tech Stack

- **Framework**: .NET 10, Blazor Server SSR
- **PGMQ**: Npgmq 1.6.0+
- **UI**: Fluent UI Blazor 4.13.2+: https://www.fluentui-blazor.net/ and https://github.com/microsoft/fluentui-blazor
- **Orchestration**: .NET Aspire 13.0 (local dev only)
- **Database**: PostgreSQL + PGMQ extension
- **Testing**: TUnit, FakeItEasy, AwesomeAssertions (fork of FluentAssertions), Testcontainers, bUnit, Playwright

## Key Commands

```bash
dotnet build && dotnet test    # Quality gates
dotnet aspire run                               # Run local dev environment with dashboard + PostgreSQL
docker build -t pgmq-admin-ui ./PgmqAdminUI     # Production build
```

## Quality Gates ✅

Before finishing task: `dotnet build` → `dotnet test` → `dotnet format` → Update `AGENTS.md` if patterns changed.

**Automated via Husky.Net**: Pre-commit hooks automatically run build, test, and format before each commit. See `.husky/README.md` for details.

## Coding Standards

### Core Principles

- **SOLID, KISS, YAGNI**. Simplicity first: inline code over new classes, direct calls over abstractions.
- **No premature abstraction**: No provider/manager/factory until 3+ use cases.
- **Self-documenting**: Descriptive names. Comments explain **why**, not **what**.
- **Async/await** for all I/O with proper error handling.

### Patterns

- **Vertical Slices**: `Features/{Name}/` contains DTOs, services, components together. Shared UI in `Components/UI/`.
- **Services**: `QueueService`/`MessageService` wrap `NpgmqClient`. No repository pattern.
- **Render Modes**: Static SSR default. Interactive Server only when needed.
- **UI**: FluentDataGrid for tables, FluentDialog for modals.

### Code Style

- No interfaces for single implementations. Concrete types suffice.
- No XML docs
- `ILogger<T>` for logging
- Required init members for DTOs (use `record`)
- Commentary: Only when intent is unclear—explain **why**, not **what**. Avoid obvious comments that restate code.
- `DateTimeOffset` > `DateTime`
- **Dependencies**:
- Managed in `Directory.Packages.props`.
- Update: `dotnet tool run dotnet-outdated -u:auto -r`.

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

## Testing

See [PgmqAdminUI.Tests/AGENTS.md](PgmqAdminUI.Tests/AGENTS.md) for test commands, patterns, and fixtures.

## Agent Workflow

**Local Dev**: `dotnet aspire run` — starts dashboard, PostgreSQL container, and app.

**Common Tasks**:

- **New operation**: Update service in `Features/`, add unit + integration tests.
- **New component**: Create in feature folder, add bUnit test. `Components/UI/` only for shared.

## Git Hooks

**Husky.Net** manages pre-commit hooks to enforce quality gates automatically.

**Setup** (required after cloning):

```bash
dotnet husky install
```

**Pre-commit checks**:

1. Build validation
2. All tests pass
3. Code formatting (staged files only)

Configuration: `.husky/task-runner.json` | Documentation: `.husky/README.md`

## Rules

- Quality gates run automatically via pre-commit hooks (Husky.Net)
- Update `AGENTS.md` if patterns change
- No temp docs (`IMPLEMENTATION.md`, `TASKS.md`). Use `AGENTS.md` and `IMPLEMENTATION_PLAN.md` only
