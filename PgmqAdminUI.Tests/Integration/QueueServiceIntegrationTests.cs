using Npgsql;
using PgmqAdminUI.Features.Queues;
using Testcontainers.PostgreSql;

namespace PgmqAdminUI.Tests.Integration;

[Property("Category", "Integration")]
[NotInParallel("SharedDatabase")]
public class QueueServiceIntegrationTests : IAsyncDisposable
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;
    private QueueService? _queueService;

    [Before(Test)]
    public async Task SetupAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("ghcr.io/pgmq/pg18-pgmq:v1.7.0")
            .WithName($"pgmq-postgres-queue-integration-tests-{Guid.NewGuid():N}")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        // Enable PGMQ extension
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS pgmq CASCADE;", connection);
        await command.ExecuteNonQueryAsync();

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QueueService>();

        _queueService = new QueueService(_connectionString, logger);
    }

    [Test]
    public async Task CreateQueue_CreatesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var queues = await _queueService.ListQueuesAsync();
        queues.Any(q => q.Name == queueName).Should().BeTrue();
    }

    [Test]
    public async Task DeleteQueue_DeletesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var deleteResult = await _queueService.DeleteQueueAsync(queueName);
        deleteResult.Should().BeTrue();

        var queues = await _queueService.ListQueuesAsync();
        queues.Any(q => q.Name == queueName).Should().BeFalse();
    }

    [Test]
    public async Task ListQueues_ReturnsAllQueues()
    {
        var queueName1 = $"test-queue-{Guid.NewGuid()}";
        var queueName2 = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName1);
        await _queueService.CreateQueueAsync(queueName2);

        using var _ = new AssertionScope();
        var queues = await _queueService.ListQueuesAsync();
        queues.Any(q => q.Name == queueName1).Should().BeTrue();
        queues.Any(q => q.Name == queueName2).Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName1);
        await _queueService.DeleteQueueAsync(queueName2);
    }

    [Test]
    public async Task GetQueueDetail_ReturnsQueueMessages()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var detail = await _queueService.GetQueueDetailAsync(queueName, 1, 10);
        detail.QueueName.Should().Be(queueName);
        detail.Messages.Should().NotBeNull();

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task GetQueueStats_ReturnsStatsForQueue()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var stats = await _queueService.GetQueueStatsAsync(queueName);
        stats.Should().NotBeNull();
        stats!.QueueName.Should().Be(queueName);
        stats.QueueLength.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalMessages.Should().BeGreaterThanOrEqualTo(0);

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task GetQueueStats_ReturnsNullAges_WhenQueueEmpty()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var stats = await _queueService.GetQueueStatsAsync(queueName);
        stats.Should().NotBeNull();
        stats!.NewestMsgAgeSec.Should().BeNull();
        stats.OldestMsgAgeSec.Should().BeNull();

        await _queueService.DeleteQueueAsync(queueName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
