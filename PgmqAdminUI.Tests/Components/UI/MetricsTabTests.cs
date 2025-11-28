using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
[Obsolete("This test class has async rendering timing issues with Fluent UI components. Tests fail because FluentMessageBar elements are not found in time. Refactor to use bUnit's WaitForAssertion or WaitForElement mechanisms instead of Task.Delay.")]
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
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        var title = cut.Find("h3");
        title.TextContent.Should().Contain("Queue Metrics");
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingMetrics()
    {
        using var _ = new AssertionScope();
        var tcs = new TaskCompletionSource<QueueStatsDto?>();
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(tcs.Task);

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        cut.FindAll("fluent-progress-ring").Count.Should().BeGreaterThan(0);

        tcs.SetResult(null);
    }

    [Test]
    public async Task DisplaysMetricsCards_WhenStatsLoaded()
    {
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        var cards = cut.FindAll("fluent-card");
        cards.Count.Should().BeGreaterThanOrEqualTo(5); // At least 5 metric cards
    }

    [Test]
    public async Task DisplaysQueueLength()
    {
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        cut.Markup.Should().Contain("42");
    }

    [Test]
    public async Task DisplaysTotalMessages()
    {
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        cut.Markup.Should().Contain("999");
    }

    [Test]
    public async Task DisplaysNA_WhenMessageAgesAreNull()
    {
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        cut.Markup.Should().Contain("N/A");
    }

    [Test]
    public async Task ShowsAutoRefreshMessage()
    {
        using var _ = new AssertionScope();
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

        await Task.Delay(100); // Wait for async initialization

        cut.Markup.Should().Contain("Auto-refreshing");
    }

    [Test]
    public async Task ShowsErrorMessage_WhenStatsIsNull()
    {
        using var _ = new AssertionScope();
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(null));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100); // Wait for async initialization

        var messageBar = cut.Find("fluent-message-bar");
        messageBar.TextContent.Should().Contain("Failed to load metrics");
    }

    [Test]
    public async Task ShowsErrorNotification_WhenLoadMetricsFails()
    {
        using var _ = new AssertionScope();
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        await Task.Delay(100); // Wait for async initialization

        A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
    }
}
