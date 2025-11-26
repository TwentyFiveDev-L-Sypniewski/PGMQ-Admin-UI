# PGMQ Admin UI Tests - Agent Guide

## Test Structure

**3-Layer Testing Pyramid** organized by feature, not by test category.

- **Categories**: `Unit`, `Integration`, `Component`
- **Marking**: `[Property("Category", "Unit|Integration|Component")]` at class level

## Running Tests

```bash
dotnet test                                      # All tests
dotnet test --filter "Category=Unit"            # Unit tests only
dotnet test --filter "Category=Integration"     # Integration tests only
dotnet test --filter "Category=Component"       # Component tests only
```

## Testing Layers

| Layer       | Stack                  | What                                  | Speed |
| ----------- | ---------------------- | ------------------------------------- | ----- |
| Unit        | TUnit + FakeItEasy     | Service methods, business logic, DTOs | ms    |
| Integration | TUnit + Testcontainers | Real PostgreSQL + PGMQ operations     | s     |
| Component   | bUnit + TUnit          | Blazor components, form submissions   | ms    |

## Configuration

### Test Fixtures

- Use TUnit `IAsyncInitializer` for container setup
- Scope: `SharedType.PerTestSession`

### Test Data & Cleanup

- **Queue Isolation**: Unique names per test: `test-queue-{Guid.NewGuid()}`
- **Cleanup**: `DeleteQueueAsync()` after each test
- **No Respawn needed**: PGMQ queues are isolated

## Common Issues

| Issue                  | Fix                                  |
| ---------------------- | ------------------------------------ |
| Docker Socket Error    | Configure `DOCKER_HOST`              |
| PGMQ Extension Missing | Use `quay.io/tembo/pg18-pgmq:latest` |
| Testcontainers Timeout | Ensure Docker running: `docker ps`   |
| Orphaned Containers    | `docker container prune`             |

## Best Practices

1. **Feature-Based Organization**: Tests by feature, categories via attributes
2. **Isolation**: Unique queue names per test
3. **Parallelism**: `[NotInParallel("SharedDatabase")]` for shared containers
4. **Sociable Tests**: Real operations over mocking
5. **Observable Outcomes**: Verify state, not mock interactions

## Reference

For coding standards and architecture, see main `AGENTS.md` in repository root.
