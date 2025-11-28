using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using PgmqAdminUI.Components.Pages;
using PgmqAdminUI.Features.Queues;

namespace PgmqAdminUI.Tests.Components.Pages;

[Property("Category", "Component")]
[Obsolete]
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
        var cut = Render<Queues>();

        var title = cut.Find("h2");
        await Assert.That(title.TextContent).IsEqualTo("Queues");
    }

    [Test]
    public async Task ShowsLoadingIndicator_WhenLoadingQueues()
    {
        var tcs = new TaskCompletionSource<IEnumerable<QueueDto>>();
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(tcs.Task);

        var cut = Render<Queues>();

        await Assert.That(cut.FindAll("fluent-progress-ring").Count).IsGreaterThan(0);

        tcs.SetResult([]);
    }

    [Test]
    public async Task DisplaysQueueList_WhenQueuesExist()
    {
        var queues = new List<QueueDto>
        {
            new() { Name = "test-queue-1", TotalMessages = 10, InFlightMessages = 2, ArchivedMessages = 5 },
            new() { Name = "test-queue-2", TotalMessages = 20, InFlightMessages = 0, ArchivedMessages = 3 }
        };

        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(queues.AsEnumerable()));

        var cut = Render<Queues>();

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        await Assert.That(cut.FindAll("fluent-data-grid").Count).IsEqualTo(1);
    }

    [Test]
    public async Task ShowsInfoMessage_WhenNoQueuesExist()
    {
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(Enumerable.Empty<QueueDto>()));

        var cut = Render<Queues>();

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        var messageBar = cut.Find("fluent-message-bar");
        await Assert.That(messageBar).IsNotNull();
    }

    [Test]
    public async Task ShowsCreateButton()
    {
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Returns(Task.FromResult(Enumerable.Empty<QueueDto>()));

        var cut = Render<Queues>();

        var buttons = cut.FindAll("fluent-button");
        var createButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Create Queue"));

        await Assert.That(createButton).IsNotNull();
    }

    [Test]
    public async Task ShowsErrorMessage_WhenLoadQueuesFails()
    {
        A.CallTo(() => _fakeQueueService.ListQueuesAsync(A<CancellationToken>._))
            .Throws(new Exception("Database connection failed"));

        var cut = Render<Queues>();

        await Task.Delay(100).ConfigureAwait(false); // Wait for async initialization

        A.CallTo(() => _fakeMessageService.ShowMessageBar(A<Action<MessageOptions>>._))
            .MustHaveHappened();
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
