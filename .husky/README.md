# Husky.NET Git Hooks

This directory contains git hooks managed by [Husky.NET](https://alirezanet.github.io/Husky.Net/).

## Setup

Husky.NET is configured as a local dotnet tool in `.config/dotnet-tools.json`.

To install the git hooks (already done for this repo):
```bash
dotnet husky install
```

## Hooks

### pre-commit
Runs before each git commit to automatically format C# code using `dotnet format`.

**Note:** The hook formats only staged files before the commit is finalized. This ensures that all committed code is properly formatted without requiring manual intervention.

## Task Configuration

Tasks are defined in `task-runner.json`. See the [Husky.NET documentation](https://alirezanet.github.io/Husky.Net/guide/task-runner.html) for more details.

## Modifying Hooks

To add a new hook:
```bash
dotnet husky add <hook-name>
```

To update task configuration, edit `task-runner.json`.
