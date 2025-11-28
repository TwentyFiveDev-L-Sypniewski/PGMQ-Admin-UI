using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.Pages;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.Pages;

[Property("Category", "Component")]
public class QueuesTests : FluentTestBase
{
    private readonly QueueService _fakeQueueService;
    private readonly IMessageService _fakeMessageService;

    public QueuesTests()
    {
        _fakeQueueService = A.Fake<QueueService>();
        _fakeMessageService = A.Fake<IMessageService>();

        Services.AddSingleton(_fakeQueueService);
        Services.AddSingleton(_fakeMessageService);
        Services.AddSingleton<NavigationManager>(new FakeNavigationManager());
    }

    [Test]
    public async Task RendersQueuesPageTitle()
    {
        // Arrange & Act
        var cut = Render<Queues>();

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var title = cut.Find("h2");
            title.TextContent.Should().Be("Queues");
        });
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingQueues()
    {
        // Arrange
        var tcs = new TaskCompletionSource<IEnumerable<QueueDto>>();
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(tcs.Task);

        // Act
        var cut = Render<Queues>();

        // Assert - should show loading indicator while waiting
        cut.FindAll("fluent-progress-ring").Count.Should().BeGreaterThan(0);

        // Cleanup - complete the task
        tcs.SetResult([]);
    }

    [Test]
    public async Task DisplaysQueueList_WhenQueuesExist()
    {
        // Arrange
        var queues = new List<QueueDto>
        {
            new() { Name = "test-queue-1", TotalMessages = 10, InFlightMessages = 2, ArchivedMessages = 5 },
            new() { Name = "test-queue-2", TotalMessages = 20, InFlightMessages = 0, ArchivedMessages = 3 }
        };

        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(queues.AsEnumerable()));

        // Act
        var cut = Render<Queues>();

        // Assert - wait for component to finish loading by checking for grid
        // FluentDataGrid renders as a table element
        await cut.WaitForStateAsync(
            () => cut.Markup.Contains("table") || cut.Markup.Contains("role=\"grid\""),
            TimeSpan.FromSeconds(3));

        // Verify the grid is rendered (table or div with grid role)
        var markup = cut.Markup;
        (markup.Contains("table") || markup.Contains("role=\"grid\"")).Should().BeTrue(
            $"Expected a table or grid element. Markup: {markup}");
    }

    [Test]
    public async Task ShowsInfoMessage_WhenNoQueuesExist()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(Enumerable.Empty<QueueDto>()));

        // Act
        var cut = Render<Queues>();

        // Assert - wait for component to finish loading
        await cut.WaitForStateAsync(
            () => !cut.Markup.Contains("fluent-progress-ring"),
            TimeSpan.FromSeconds(3));

        // FluentMessageBar renders with class "fluent-messagebar" not as <fluent-message-bar> custom element
        cut.Markup.Should().Contain("fluent-messagebar");
    }

    [Test]
    public async Task ShowsCreateButton()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(Enumerable.Empty<QueueDto>()));

        // Act
        var cut = Render<Queues>();

        // Assert
        await cut.WaitForAssertionAsync(() =>
        {
            var buttons = cut.FindAll("fluent-button");
            var createButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Create Queue"));
            createButton.Should().NotBeNull();
        });
    }

    [Test]
    public async Task ShowsErrorMessage_WhenLoadQueuesFails()
    {
        // Arrange
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        // Act
        var cut = Render<Queues>();

        // Assert - verify error notification was shown
        await cut.WaitForAssertionAsync(() =>
        {
            A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
                .MustHaveHappened();
        });
    }

    private class FakeNavigationManager : NavigationManager
    {
        public FakeNavigationManager() => Initialize("https://localhost:5001/", "https://localhost:5001/");

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // No-op for testing
        }
    }
}
