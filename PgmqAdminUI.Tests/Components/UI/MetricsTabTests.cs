using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete]
public class MetricsTabTests : FluentTestBase
{
    private readonly QueueService _fakeQueueService;
    private readonly IMessageService _fakeMessageService;

    public MetricsTabTests()
    {
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
    }

    [Test]
    public async Task RendersMetricsTabTitle()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 10,
                TotalMessages = 100,
                OldestMsgAgeSec = 60,
                NewestMsgAgeSec = 5,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var title = cut.Find("h3");
        await Assert.That(title.TextContent).Contains("Queue Metrics");
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingMetrics()
    {
        var tcs = new TaskCompletionSource<QueueStatsDto?>();
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(tcs.Task);

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Assert.That(cut.FindAll("fluent-progress-ring").Count).IsGreaterThan(0);

        tcs.SetResult(null);
    }

    [Test]
    public async Task DisplaysMetricsCards_WhenStatsLoaded()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 10,
                TotalMessages = 100,
                OldestMsgAgeSec = 60,
                NewestMsgAgeSec = 5,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var cards = cut.FindAll("fluent-card");
        await Assert.That(cards.Count).IsGreaterThanOrEqualTo(5); // At least 5 metric cards
    }

    [Test]
    public async Task DisplaysQueueLength()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 42,
                TotalMessages = 100,
                OldestMsgAgeSec = 60,
                NewestMsgAgeSec = 5,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.Markup).Contains("42");
    }

    [Test]
    public async Task DisplaysTotalMessages()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 10,
                TotalMessages = 999,
                OldestMsgAgeSec = 60,
                NewestMsgAgeSec = 5,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.Markup).Contains("999");
    }

    [Test]
    public async Task DisplaysNA_WhenMessageAgesAreNull()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 0,
                TotalMessages = 0,
                OldestMsgAgeSec = null,
                NewestMsgAgeSec = null,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.Markup).Contains("N/A");
    }

    [Test]
    public async Task ShowsAutoRefreshMessage()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(new QueueStatsDto
            {
                QueueName = "test-queue",
                QueueLength = 10,
                TotalMessages = 100,
                OldestMsgAgeSec = 60,
                NewestMsgAgeSec = 5,
                ScrapeTime = DateTimeOffset.UtcNow
            }));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.Markup).Contains("Auto-refreshing");
    }

    [Test]
    public async Task ShowsErrorMessage_WhenStatsIsNull()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(null));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var messageBar = cut.Find("fluent-message-bar");
        await Assert.That(messageBar.TextContent).Contains("Failed to load metrics");
    }

    [Test]
    public async Task ShowsErrorNotification_WhenLoadMetricsFails()
    {
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
