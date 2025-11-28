using Bunit;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.UI;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.UI;

[Property("Category", "Component")]
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
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h3");
            title.TextContent.Should().Contain("Queue Metrics");
        });
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingMetrics()
    {
        // Arrange
        var tcs = new TaskCompletionSource<QueueStatsDto?>();
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(tcs.Task);

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - should show loading indicator while waiting
        cut.FindAll("fluent-progress-ring").Count.Should().BeGreaterThan(0);

        // Cleanup - complete the task
        tcs.SetResult(null);
    }

    [Test]
    public async Task DisplaysMetricsCards_WhenStatsLoaded()
    {
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var cards = cut.FindAll("fluent-card");
            cards.Count.Should().BeGreaterThanOrEqualTo(5); // At least 5 metric cards
        });
    }

    [Test]
    public async Task DisplaysQueueLength()
    {
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("42");
        });
    }

    [Test]
    public async Task DisplaysTotalMessages()
    {
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("999");
        });
    }

    [Test]
    public async Task DisplaysNA_WhenMessageAgesAreNull()
    {
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("N/A");
        });
    }

    [Test]
    public async Task ShowsAutoRefreshMessage()
    {
        // Arrange
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

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            cut.Markup.Should().Contain("Auto-refreshing");
        });
    }

    [Test]
    public async Task ShowsErrorMessage_WhenStatsIsNull()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<QueueStatsDto?>(null));

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - wait for component to finish loading
        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // FluentMessageBar renders with class "fluent-messagebar"
        cut.Markup.Should().Contain("fluent-messagebar");
        cut.Markup.Should().Contain("Failed to load metrics");
    }

    [Test]
    public async Task ShowsErrorNotification_WhenLoadMetricsFails()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.GetQueueStatsAsync(A<string>._, A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        // Act
        var cut = Render<MetricsTab>(parameters => parameters
            .Add(p => p.QueueName, "test-queue"));

        // Assert - verify error notification was shown
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }
}
