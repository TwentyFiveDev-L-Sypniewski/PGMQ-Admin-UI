# PGMQ Admin UI Tests - Agent Guide

## Test Structure

**3-Layer Testing Pyramid:**

- **`PgmqAdminUI.Tests/`**: Unified test project (Unit, Integration, Component)
  - **`Unit/`**: Fast unit tests with mocked dependencies (TUnit + FakeItEasy)
  - **`Integration/`**: Real PostgreSQL + PGMQ operations (TUnit + Testcontainers)
  - **`Component/`**: Blazor component tests (bUnit + TUnit)
- **Categories**: `Unit`, `Integration`, `Component`

## Running Tests

```bash
# All Tests
dotnet test

# Filtered by Category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Component"
```

## Testing Layers

### Layer 1: Unit Tests (TUnit + FakeItEasy)

**What:** PgmqService methods, business logic, DTOs
**How:** Mock NpgmqClient interface with FakeItEasy
**Speed:** Milliseconds
**Location:** `Unit/Services/PgmqServiceTests.cs`

**Example:**
```csharp
[Test]
public async Task SendMessageAsync_ValidQueue_ReturnsMessageId()
{
    // Arrange
    var fakeClient = A.Fake<INpgmqClient>();
    A.CallTo(() => fakeClient.SendAsync("test-queue", A<string>._, null))
        .Returns("msg_123");

    var service = new PgmqService(fakeClient);

    // Act
    var result = await service.SendMessageAsync("test-queue", "{\"data\":\"test\"}");

    // Assert
    result.ShouldBe("msg_123");
}
```

### Layer 2: Integration Tests (TUnit + Testcontainers)

**What:** Real PostgreSQL + PGMQ operations, HTTP endpoints
**How:** Testcontainers spins up PostgreSQL with PGMQ extension
**Speed:** Seconds
**Location:** `Integration/Services/PgmqServiceIntegrationTests.cs`

**Example:**
```csharp
[Test]
public async Task SendMessage_ToRealQueue_CanBeRead()
{
    // Arrange - Testcontainers PostgreSQL with PGMQ
    await using var container = new PostgreSqlBuilder()
        .WithImage("quay.io/tembo/pg18-pgmq:latest")
        .Build();
    await container.StartAsync();

    var connectionString = container.GetConnectionString();
    var pgmq = new NpgmqClient(connectionString);

    var queueName = $"test-queue-{Guid.NewGuid()}";
    await pgmq.CreateQueueAsync(queueName);

    // Act
    var msgId = await pgmq.SendAsync(queueName, "{\"test\":\"data\"}");
    var message = await pgmq.ReadAsync(queueName, vt: 30);

    // Assert
    message.ShouldNotBeNull();
    message.MsgId.ShouldBe(msgId);

    // Cleanup
    await pgmq.DeleteQueueAsync(queueName);
}
```

### Layer 3: Component Tests (bUnit + TUnit)

**What:** Blazor components (QueueGrid, MessageForm, Pages)
**How:** bUnit renders components, simulates button clicks, form submissions
**Speed:** Milliseconds
**Location:** `Component/UI/QueueGridTests.cs`

**Note:** bUnit works with both static and interactive SSR components

**Example:**
```csharp
[Test]
public void QueueGrid_RendersQueueList_DisplaysQueueNames()
{
    // Arrange
    using var ctx = new TestContext();
    var queues = new[] { new QueueDto { Name = "test-queue", MessageCount = 5 } };

    // Act
    var component = ctx.RenderComponent<QueueGrid>(parameters => parameters
        .Add(p => p.Queues, queues));

    // Assert
    var row = component.Find("td");
    row.TextContent.ShouldContain("test-queue");
}
```

## Configuration & Setup

### Docker Requirements

- **Required for:** Integration tests (Testcontainers)
- **Image:** `quay.io/tembo/pg18-pgmq:latest` (PostgreSQL 18 with PGMQ extension)
- **Rootless Docker:** Configure `DOCKER_HOST=unix:///run/user/1000/docker.sock` if needed
- **Verify Docker:** Run `docker ps` to ensure Docker daemon is running

### Test Fixtures

- **Pattern:** TUnit `IAsyncInitializer` for test container setup
- **Scope:** Shared test session (`SharedType.PerTestSession`)
- **Locking:** Not needed - TUnit handles initialization automatically

**Example Fixture:**
```csharp
public class PostgreSqlTestContainerFixture : IAsyncInitializer
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("quay.io/tembo/pg18-pgmq:latest")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }
}
```

### Test Data & Cleanup

- **Test Data:** Inline test data or helpers in `TestHelpers/`
- **Queue Isolation:** Create unique queue names per test: `test-queue-{Guid.NewGuid()}`
- **Cleanup:** Delete queues after each test via `DeleteQueueAsync()`
- **No Respawn:** PGMQ queues are isolated - no need for database reset tools

## Common Issues

| Issue | Fix |
|-------|-----|
| Docker Socket Error | Configure `DOCKER_HOST` (rootless: `unix:///run/user/1000/docker.sock`) |
| Port in Use | `docker stop <id>` or `docker ps` to find conflicts |
| PGMQ Extension Missing | Verify Testcontainers uses `quay.io/tembo/pg18-pgmq:latest` |
| Testcontainers Timeout | Ensure Docker is running: `docker ps` |
| Connection Refused | Check container startup logs: `docker logs <id>` |
| Orphaned Containers | `docker rm -f <id>` or `docker container prune` |
| Aspire Dashboard Not Loading | Verify port 17287 available, restart AppHost |

## Best Practices

1. **PostgreSQL Testcontainers**: Use `quay.io/tembo/pg18-pgmq:latest` for all integration tests
2. **Isolation**: Create/delete queues per test with unique names (no Respawn needed)
3. **Parallelism**: Use `[NotInParallel("SharedDatabase")]` for integration tests sharing container
4. **Naming**: Descriptive test names via method names or attributes
5. **Assertions**: Use TUnit's built-in assertions (`ShouldBe`, `ShouldNotBeNull`, etc.)
6. **Sociable Tests**: Exercise real PGMQ operations; avoid over-mocking
7. **Observable Outcomes**: Verify queue state, message content - not just mock interactions
8. **No Redundancy**: Don't duplicate integration coverage with unit tests

## PGMQ Testing Patterns

### Creating Test Queues
```csharp
var queueName = $"test-queue-{Guid.NewGuid()}";
await pgmq.CreateQueueAsync(queueName);
```

### Sending Test Messages
```csharp
var testData = new { UserId = 123, Action = "test" };
var msgId = await pgmq.SendAsync(queueName, JsonSerializer.Serialize(testData));
```

### Reading Messages
```csharp
var message = await pgmq.ReadAsync<TestDto>(queueName, vt: 30);
message.ShouldNotBeNull();
message.Message.ShouldBe(testData);
```

### Archiving Messages
```csharp
await pgmq.ArchiveAsync(queueName, msgId);
// Verify in archive table (pgmq.a_*)
```

### Cleanup After Tests
```csharp
await pgmq.DeleteQueueAsync(queueName);
```

### Archive Verification
```csharp
// Query archive table directly
await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
var sql = $"SELECT COUNT(*) FROM pgmq.a_{queueName}";
var count = await connection.ExecuteScalarAsync<int>(sql);
count.ShouldBe(1);
```

## Blazor Component Testing (bUnit)

### Component Test Setup
```csharp
using var ctx = new TestContext();
```

### Mock Services
```csharp
var mockPgmqService = A.Fake<IPgmqService>();
ctx.Services.AddSingleton(mockPgmqService);
```

### Render Component
```csharp
var component = ctx.RenderComponent<QueueGrid>(parameters => parameters
    .Add(p => p.Queues, testQueues));
```

### Simulate Button Click
```csharp
var button = component.Find("button.delete-queue");
await button.ClickAsync(new MouseEventArgs());
```

### Form Submission
```csharp
var form = component.Find("form");
await form.SubmitAsync();
```

### Verify Rendered Output
```csharp
var tableRow = component.Find("tr.queue-row");
tableRow.TextContent.ShouldContain("test-queue");
```

### Works with SSR
bUnit supports both static and interactive SSR Blazor components. No special configuration needed.

## Integration with Aspire

For integration tests that need the full application stack:

1. **Local Development:** `dotnet run --project PgmqAdminUI.AppHost`
2. **Aspire Dashboard:** https://localhost:17287
3. **PostgreSQL:** Automatically started by Aspire with PGMQ extension
4. **App URL:** Retrieved from Aspire dashboard or configuration

## CI/CD

- CI runs all tests automatically using Docker
- Use category filters to optimize run times:
  - Fast feedback: `dotnet test --filter "Category=Unit"`
  - Full suite: `dotnet test`
- Testcontainers works in CI environments with Docker support

## Reference

For coding standards, patterns, and architecture guidance, see the main `AGENTS.md` file in the repository root.

**Key Principles:**
- SOLID, KISS, YAGNI
- No premature abstraction
- Self-documenting code
- Async/await for all I/O
