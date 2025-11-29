# PGMQ Admin UI Tests - Agent Guide

## Test Structure

**Feature-First Organization** - organize by feature (vertical slices), not by test type.

**Organization:** Tests in `Features/{FeatureName}/` folders (e.g., Features/Messages/, Features/Queues/). Shared component tests in `Components/` folder. Use `[Property("Category", "Unit|Integration|Component")]` at class level to distinguish test types.

**Key Rules:**

- Organize by **feature first**, not by test category
- NO separate `/Unit/`, `/Integration/`, `/Component/` root folders
- Filter via `dotnet test -- --treenode-filter "/*/*/*/*[Category=Integration]"`
- **Categories**: `Unit`, `Integration`, `Component`

**Note:** TUnit uses `--treenode-filter` ([docs](https://tunit.dev/docs/execution/test-filters/)). The `--` separates dotnet test args from TUnit args. Pattern follows the test tree hierarchy: `/Assembly/Namespace/Class/Test` — use `*` wildcards to match any segment (e.g., `/*/*/*/*[Category=Unit]` matches all tests with Category=Unit).

## Running Tests

```bash
dotnet test                                                        # All tests
dotnet test -- --treenode-filter "/*/*/*/*[Category=Component]"    # Component tests only
dotnet test -- --treenode-filter "/*/*/ClassName/*"                # All tests in a class
dotnet test -- --treenode-filter "/*/Namespace/Class/TestMethod"   # Single specific test
```

## Testing Layers

| Layer       | Stack                  | What                                | When to Use                          | Speed |
| ----------- | ---------------------- | ----------------------------------- | ------------------------------------ | ----- |
| Unit        | TUnit + FakeItEasy     | Business logic, parsing, validation | Service has logic beyond I/O calls   | ms    |
| Integration | TUnit + Testcontainers | Real PostgreSQL + PGMQ operations   | Services wrap external dependencies  | s     |
| Component   | bUnit + TUnit          | Blazor components, form submissions | UI components with user interactions | ms    |

**Decision Tree:**

- Does the service have business logic (validation, transformation, complex conditionals)?
  - YES → Unit tests for that logic
  - NO → Skip unit tests, use integration tests only
- Does the service call external dependencies (DB, PGMQ, HTTP)?
  - YES → Integration tests with real dependencies (Testcontainers)
  - NO → Unit tests may suffice

## Configuration

### Test Data & Cleanup

- **Queue Isolation**: Unique names per test: `test-queue-{Guid.NewGuid()}`
- **Database Reset**: Use Respawn via `fixture.ResetDatabaseAsync()` in `[Before(Test)]` to clean PGMQ tables
- **Respawn Config**: Targets `pgmq` schema, ignores `meta` table, recreates dynamically (PGMQ creates tables on-the-fly)

## Anti-Patterns to Avoid

**Mock-Only Tests:**

- ❌ NEVER create tests that fake a service and verify the fake's behavior
- ❌ NEVER test services by mocking the service itself
- ✅ Test real logic with unit tests OR use integration tests with real dependencies

**When to Use Mocks vs Integration:**

- Unit tests with mocks: Only when service has significant business logic to isolate
- Integration tests: For services wrapping external libraries/databases (QueueService, MessageService)
- Rule: If service just calls external client methods, use integration tests only

## Best Practices

1. **Feature-First Organization**: Tests in `Features/{FeatureName}/`, not `/Unit/` or `/Integration/`
2. **No Mock-Only Tests**: If service is thin wrapper, use integration tests only
3. **Test Real Behavior**: Verify observable outcomes (DB state, return values), not mock interactions
4. **PostgreSQL Testcontainers**: Use for integration tests with real DB interactions
5. **Isolation**: Unique queue names per test: `test-queue-{Guid.NewGuid()}`
6. **Parallelism**: Use `[NotInParallel("SharedDatabase")]` for integration tests
7. **Naming**: Descriptive via `[DisplayName]` or pattern: `MethodName_Scenario_ExpectedOutcome`
8. **Assertions**: Use AwesomeAssertions with `AssertionScope`
9. **Sociable Tests**: Exercise real collaborators; avoid over-mocking
10. **No Redundancy**: Don't duplicate integration coverage with unit tests

## Bug-Fix Testing Pattern

When fixing a bug, write tests FIRST that assert the expected (correct) behavior:

1. **Write production-grade tests**: Assert what the code SHOULD do, not "reproduce the bug"
2. **Tests fail initially**: Confirms the bug exists
3. **Implement the fix**: Tests now pass
4. **No "bug" comments**: Tests should read like normal feature tests — no "REPRODUCES THE ISSUE" or "CURRENT BUG" language

Example: If clicking a button triggers wrong action, write a test asserting the correct action. Test fails → confirms bug. Fix code → test passes.

## Reference

For coding standards and architecture, see main `AGENTS.md` in repository root.
