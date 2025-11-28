using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Integration;

[Property("Category", "Integration")]
[NotInParallel("SharedDatabase")]
[ClassDataSource<PostgresFixture>(Shared = SharedType.Keyed, Key = "SharedDatabase")]
public class QueueServiceIntegrationTests(PostgresFixture fixture)
{
    private QueueService? _sut;

    [Before(Test)]
    public async Task SetupAsync()
    {
        await fixture.ResetDatabaseAsync();
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QueueService>();
        _sut = new QueueService(fixture.PostgresConnectionString, logger);
    }

    [Test]
    public async Task CreateQueue_CreatesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var queues = await _sut.ListQueuesAsync();
        queues.Any(q => q.Name == queueName).Should().BeTrue();
    }

    [Test]
    public async Task DeleteQueue_DeletesQueueSuccessfully()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var deleteResult = await _sut.DeleteQueueAsync(queueName);
        deleteResult.Should().BeTrue();

        var queues = await _sut.ListQueuesAsync();
        queues.Any(q => q.Name == queueName).Should().BeFalse();
    }

    [Test]
    public async Task ListQueues_ReturnsAllQueues()
    {
        var queueName1 = $"test-queue-{Guid.NewGuid()}";
        var queueName2 = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName1);
        await _sut.CreateQueueAsync(queueName2);

        using var _ = new AssertionScope();
        var queues = await _sut.ListQueuesAsync();
        queues.Any(q => q.Name == queueName1).Should().BeTrue();
        queues.Any(q => q.Name == queueName2).Should().BeTrue();

        await _sut.DeleteQueueAsync(queueName1);
        await _sut.DeleteQueueAsync(queueName2);
    }

    [Test]
    public async Task GetQueueDetail_ReturnsQueueMessages()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var detail = await _sut.GetQueueDetailAsync(queueName, 1, 10);
        detail.QueueName.Should().Be(queueName);
        detail.Messages.Should().NotBeNull();

        await _sut.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task GetQueueStats_ReturnsStatsForQueue()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var stats = await _sut.GetQueueStatsAsync(queueName);
        stats.Should().NotBeNull();
        stats!.QueueName.Should().Be(queueName);
        stats.QueueLength.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalMessages.Should().BeGreaterThanOrEqualTo(0);

        await _sut.DeleteQueueAsync(queueName);
    }

    [Test]
    public async Task GetQueueStats_ReturnsNullAges_WhenQueueEmpty()
    {
        var queueName = $"test-queue-{Guid.NewGuid()}";

        await _sut!.CreateQueueAsync(queueName);

        using var _ = new AssertionScope();
        var stats = await _sut.GetQueueStatsAsync(queueName);
        stats.Should().NotBeNull();
        stats!.NewestMsgAgeSec.Should().BeNull();
        stats.OldestMsgAgeSec.Should().BeNull();

        await _sut.DeleteQueueAsync(queueName);
    }
}
