using Npgsql;
using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;
using Testcontainers.PostgreSql;

namespace PgmqAdminUI.Tests.Integration;

[Property("Category", "Integration")]
[NotInParallel("SharedDatabase")]
public class MessageServiceIntegrationTests : IAsyncDisposable
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
            .WithName($"pgmq-postgres-message-integration-tests-{Guid.NewGuid():N}")
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

        var queueLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QueueService>();
        var messageLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MessageService>();

        _queueService = new QueueService(_connectionString, queueLogger);
        _messageService = new MessageService(_connectionString, messageLogger);
    }

    [Test]
    public async Task SendMessage_SendsMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage);
        msgId.Should().BeGreaterThan(0);

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task SendMessage_IncrementsQueueStats()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var statsBefore = await _queueService.GetQueueStatsAsync(queueName);
        await _messageService!.SendMessageAsync(queueName, JsonMessage);
        var statsAfter = await _queueService.GetQueueStatsAsync(queueName);
        statsAfter!.TotalMessages.Should().BeGreaterThan(statsBefore!.TotalMessages);

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task SendMessageWithDelay_SendsMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";
        const int DelaySeconds = 5;

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage, DelaySeconds);
        msgId.Should().BeGreaterThan(0);

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task DeleteMessage_DeletesMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage);
        var deleteResult = await _messageService.DeleteMessageAsync(queueName, msgId);
        deleteResult.Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task ArchiveMessage_ArchivesMessageSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage = "{\"data\":\"test\"}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var msgId = await _messageService!.SendMessageAsync(queueName, JsonMessage);
        var archiveResult = await _messageService.ArchiveMessageAsync(queueName, msgId);
        archiveResult.Should().BeTrue();

        await _queueService.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task CompleteWorkflow_CreateSendDeleteQueue()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";
        const string JsonMessage1 = "{\"order_id\":1001}";
        const string JsonMessage2 = "{\"order_id\":1002}";

        await _queueService!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var msgId1 = await _messageService!.SendMessageAsync(queueName, JsonMessage1);
        var msgId2 = await _messageService.SendMessageAsync(queueName, JsonMessage2);

        var stats = await _queueService.GetQueueStatsAsync(queueName);
        stats!.TotalMessages.Should().BeGreaterThanOrEqualTo(2);

        var deleteResult1 = await _messageService.DeleteMessageAsync(queueName, msgId1);
        var archiveResult2 = await _messageService.ArchiveMessageAsync(queueName, msgId2);
        deleteResult1.Should().BeTrue();
        archiveResult2.Should().BeTrue();

        var queueDeleteResult = await _queueService.DeleteQueueAsync(queueName);
        queueDeleteResult.Should().BeTrue();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
