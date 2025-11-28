using PgmqAdminUI.Features.Messages;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Integration;

[Property("Category", "Integration")]
[NotInParallel("SharedDatabase")]
[ClassDataSource<PostgresFixture>(Shared = SharedType.Keyed, Key = "SharedDatabase")]
public class MessageServiceIntegrationTests(PostgresFixture fixture)
{
    private QueueService? _queueService;
    private MessageService? _messageService;

    [Before(Test)]
    public Task SetupAsync()
    {
        var queueLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QueueService>();
        var messageLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MessageService>();

        _queueService = new QueueService(fixture.PostgresConnectionString, queueLogger);
        _messageService = new MessageService(fixture.PostgresConnectionString, messageLogger);
        return Task.CompletedTask;
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
}
