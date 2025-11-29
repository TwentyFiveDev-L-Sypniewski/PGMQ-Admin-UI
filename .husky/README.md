# Husky.NET Git Hooks

This directory contains git hooks managed by [Husky.NET](https://alirezanet.github.io/Husky.Net/).

## Overview

Husky.NET automates code quality checks before commits, ensuring all committed code meets project standards.

## Setup

Husky.NET is configured as a local dotnet tool in `.config/dotnet-tools.json`.

To install the git hooks (required after cloning the repository):
```bash
dotnet husky install
```

## Quality Gates

The pre-commit hook runs these quality gates in sequence:

1. **Build** - `dotnet build --no-restore`
2. **Format** - `dotnet format --no-restore --include ${staged}` (staged files only)

Both must pass before the commit is finalized. Formatting changes are automatically staged.

**Note**: Tests are run separately via `dotnet test` to allow developers to optimize commit time. Run tests manually before pushing to ensure code quality.

## Task Configuration

Tasks are defined in `task-runner.json` with the following structure:

- **build**: Compiles the project to catch compilation errors
- **format**: Formats only staged C# files, project files, and props files

See the [Husky.NET Task Runner documentation](https://alirezanet.github.io/Husky.Net/guide/task-runner.html) for advanced configuration options.

## Manual Testing

Test the pre-commit hooks without making a commit:

```bash
dotnet husky run --group pre-commit
```

Test individual tasks:

```bash
dotnet husky run --name build
dotnet husky run --name format
```

Run tests manually:

```bash
dotnet test
```

## Modifying Hooks

To add a new hook:
```bash
dotnet husky add <hook-name> -c "your command here"
```

Available git hooks: `pre-commit`, `pre-push`, `commit-msg`, `pre-rebase`, `post-merge`, etc.

To modify task configuration, edit `task-runner.json`.

## Troubleshooting

**Hooks not running:**
- Ensure `dotnet husky install` has been executed
- Verify git config: `git config core.hooksPath` should output `.husky`

**Build/format failures:**
- Run quality gates manually: `dotnet build && dotnet format`
- Fix reported issues before committing

**Test failures:**
- Run tests separately: `dotnet test`
- Fix test failures before pushing to remote

**Format changes not staged:**
- The hook automatically runs `git add -u` after formatting
- Verify changes with `git status` before retrying commit
