
# PGMQ Admin UI - Development Guide

**Stack**: .NET 10, ASP.NET Core Minimal APIs, .NET Aspire 13, PostgreSQL, PGMQ, Blazor
**Principles**: SOLID, KISS, YAGNI. Consistency over innovation.

## Project Overview

Cloud-native .NET 10 modular monolith using Vertical Slice Architecture, Aspire 13, PostgreSQL, and PGMQ.

## Repository Structure

- `StockStorage/src/`: Main app (Features/, Database/, Infrastructure/)
- `StockStorage/tests/`: Unified test project (Unit, Integration, System). **See `StockStorage/tests/AGENTS.md`**.
- `StockStorage.AppHost/`: .NET Aspire orchestration
- `StockStorage.ServiceDefaults/`: Shared Aspire defaults

## Key Commands

```bash
# Core
dotnet build
dotnet test # Requires Docker
dotnet format

# EF Core
dotnet ef migrations add <Name> --project StockStorage/src/StockStorage.csproj
dotnet ef database update --project StockStorage/src/StockStorage.csproj

# Aspire
dotnet run --project StockStorage.AppHost
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

### Architecture & Examples

- **Modular Monolith**: Features implement `IModule`. See `TickerProviderFeature.cs`.
- **Minimal API**: Static handlers, `IEndpointRouteBuilder`. See `ProcessTickersEndpoint.cs`.
- **Aspire**: `AppHost` orchestrates resources. See `AppHost.cs`.
- **EF Core**: Repository pattern, Bulk extensions. See `SqlServerTickersRepository.cs`.

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

## Documentation

**MUST CHECK after every change:**

- Review `AGENTS.md` for accuracy.
- Update patterns, tools, commands, or workflows if changed.
- Ensure future developers/agents have accurate guidance.

## Testing

**Refer to `StockStorage/tests/AGENTS.md` for all testing guidelines.**

- Stack: TUnit, AwesomeAssertions, Testcontainers.
- Categories: Unit, Integration, System.

## Agent Workflow

- **Aspire**: Run `AppHost` to start full stack (API + DB + Dashboard).
- **PGMQ**: `PgmqMessagePublisher` (pub), `BackgroundService` (sub).
- **Migrations**: Add -> Review -> Update -> Test (Integration).
- **No Temp Docs**: Do not commit `IMPLEMENTATION.md` etc.
