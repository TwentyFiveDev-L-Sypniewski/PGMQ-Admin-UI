using Npgsql;
using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;
using Testcontainers.PostgreSql;

namespace PgmqAdminUI.Tests.Integration;

[Property("Category", "Integration")]
[NotInParallel("SharedDatabase")]
public class ServiceIntegrationTests : IAsyncDisposable
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;
    private QueueService? _queueService;
    private MessageService? _messageService;

    [Before(Test)]
    public async Task SetupAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("ghcr.io/pgmq/pg18-pgmq:v1.7.0")
            .WithName("pgmq-postgres-pgmq-admin-ui-integration-tests")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _container.StartAsync().ConfigureAwait(false);

        _connectionString = _container.GetConnectionString();

        // Enable PGMQ extension
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        await using var command = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS pgmq CASCADE;", connection);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);

        var queueLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QueueService>();
        var messageLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MessageService>();

        _queueService = new QueueService(_connectionString, queueLogger);
        _messageService = new MessageService(_connectionString, messageLogger);
    }

    [Test]
    public async Task QueueService_CreateQueue_CreatesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var queues = await _queueService.ListQueuesAsync().ConfigureAwait(false);
        queues.Any(q => q.Name == queueName).Should().BeTrue();
    }

    [Test]
    public async Task QueueService_DeleteQueue_DeletesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var deleteResult = await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
        deleteResult.Should().BeTrue();

        var queues = await _queueService.ListQueuesAsync().ConfigureAwait(false);
        queues.Any(q => q.Name == queueName).Should().BeFalse();
    }

    [Test]
    public async Task QueueService_ListQueues_ReturnsAllQueues()
    {
        var queueName1 = $"test-queue-{Guid.NewGuid()}";
        var queueName2 = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName1).ConfigureAwait(false);
        await _queueService.CreateQueueAsync(queueName2).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var queues = await _queueService.ListQueuesAsync().ConfigureAwait(false);
        queues.Any(q => q.Name == queueName1).Should().BeTrue();
        queues.Any(q => q.Name == queueName2).Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName1).ConfigureAwait(false);
        await _queueService.DeleteQueueAsync(queueName2).ConfigureAwait(false);
    }

    [Test]
    public async Task QueueService_GetQueueDetail_ReturnsQueueMessages()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var detail = await _queueService.GetQueueDetailAsync(queueName, 1, 10).ConfigureAwait(false);
        detail.QueueName.Should().Be(queueName);
        detail.Messages.Should().NotBeNull();

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task QueueService_GetQueueStats_ReturnsStatsForQueue()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var stats = await _queueService.GetQueueStatsAsync(queueName).ConfigureAwait(false);
        stats.Should().NotBeNull();
        stats!.QueueName.Should().Be(queueName);
        stats.QueueLength.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalMessages.Should().BeGreaterThanOrEqualTo(0);

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task QueueService_GetQueueStats_ReturnsNullAges_WhenQueueEmpty()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var stats = await _queueService.GetQueueStatsAsync(queueName).ConfigureAwait(false);
        stats.Should().NotBeNull();
        stats!.NewestMsgAgeSec.Should().BeNull();
        stats.OldestMsgAgeSec.Should().BeNull();

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task MessageService_SendMessage_SendsMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage).ConfigureAwait(false);
        msgId.Should().BeGreaterThan(0);

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task MessageService_SendMessage_IncrementsQueueStats()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var statsBefore = await _queueService.GetQueueStatsAsync(queueName).ConfigureAwait(false);
        await _messageService!.SendMessageAsync(queueName, JsonMessage).ConfigureAwait(false);
        var statsAfter = await _queueService.GetQueueStatsAsync(queueName).ConfigureAwait(false);
        statsAfter!.TotalMessages.Should().BeGreaterThan(statsBefore!.TotalMessages);

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task MessageService_SendMessageWithDelay_SendsMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";
        const int DelaySeconds = 5;

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage, DelaySeconds).ConfigureAwait(false);
        msgId.Should().BeGreaterThan(0);

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task MessageService_DeleteMessage_DeletesMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage).ConfigureAwait(false);
        var deleteResult = await _messageService.DeleteMessageAsync(queueName, msgId).ConfigureAwait(false);
        deleteResult.Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task MessageService_ArchiveMessage_ArchivesMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage).ConfigureAwait(false);
        var archiveResult = await _messageService.ArchiveMessageAsync(queueName, msgId).ConfigureAwait(false);
        archiveResult.Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
    }

    [Test]
    public async Task Integration_CompleteWorkflow_CreateSendDeleteQueue()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage1 = "{\"order_id\":1001}";
        const string JsonMessage2 = "{\"order_id\":1002}";

        await _queueService!.CreateQueueAsync(queueName).ConfigureAwait(false);

        using var _ = new AssertionScope();
        var msgId1 = await _messageService!.SendMessageAsync(queueName, JsonMessage1).ConfigureAwait(false);
        var msgId2 = await _messageService.SendMessageAsync(queueName, JsonMessage2).ConfigureAwait(false);

        var stats = await _queueService.GetQueueStatsAsync(queueName).ConfigureAwait(false);
        stats!.TotalMessages.Should().BeGreaterThanOrEqualTo(2);

        var deleteResult1 = await _messageService.DeleteMessageAsync(queueName, msgId1).ConfigureAwait(false);
        var archiveResult2 = await _messageService.ArchiveMessageAsync(queueName, msgId2).ConfigureAwait(false);
        deleteResult1.Should().BeTrue();
        archiveResult2.Should().BeTrue();

        var queueDeleteResult = await _queueService.DeleteQueueAsync(queueName).ConfigureAwait(false);
        queueDeleteResult.Should().BeTrue();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
